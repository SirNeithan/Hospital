using HospitalManagement.Data;
using HospitalManagement.Services;
using HospitalWeb.Authorization;
using HospitalWeb.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

// Tell Npgsql to handle unspecified DateTime as local
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// ── Database ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<HospitalDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Authentication (cookie) ───────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath         = "/Admin/Auth/Login";
        options.LogoutPath        = "/Admin/Auth/Logout";
        options.AccessDeniedPath  = "/Admin/Auth/AccessDenied";
        options.ExpireTimeSpan    = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly   = true;
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
        options.Cookie.Name       = "HMS.Session";
    });

// ── Authorization: named policies ─────────────────────────────────────────
//
//  AdminOnly       – must be logged in, active in DB, and have the Admin role
//  PatientOnly     – must be logged in, active in DB, and have the Patient role
//  AuthenticatedUser – any logged-in user that is still active
//
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy
        .RequireAuthenticatedUser()
        .RequireRole("Admin")
        .AddRequirements(new ActiveUserRequirement()));

    options.AddPolicy("PatientOnly", policy => policy
        .RequireAuthenticatedUser()
        .RequireRole("Patient")
        .AddRequirements(new ActiveUserRequirement()));

    options.AddPolicy("AuthenticatedUser", policy => policy
        .RequireAuthenticatedUser()
        .AddRequirements(new ActiveUserRequirement()));
});

// Register the custom authorization handler (singleton — creates its own DB scope)
builder.Services.AddSingleton<IAuthorizationHandler, ActiveUserHandler>();

// ── Application services (scoped = one per request) ───────────────────────
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<DoctorService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<MedicalRecordService>();
builder.Services.AddScoped<BillingService>();
builder.Services.AddScoped<DataStore>();

// ── App build ────────────────────────────────────────────────────────────
var app = builder.Build();

// ── DB migration + seeding on startup ────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HospitalDbContext>();
    db.Database.Migrate();

    // Replace old placeholder seed data if present
    if (db.Doctors.Any(d => d.Email.Contains("hospital.com")))
    {
        db.Appointments.RemoveRange(db.Appointments);
        db.MedicalRecords.RemoveRange(db.MedicalRecords);
        db.Bills.RemoveRange(db.Bills);
        db.Patients.RemoveRange(db.Patients);
        db.Doctors.RemoveRange(db.Doctors);
        db.Rooms.RemoveRange(db.Rooms);
        db.SaveChanges();
    }

    if (!db.Doctors.Any())
    {
        new DbSeeder(db).Seed();
    }
    else
    {
        new DbSeeder(db).SeedAdminsOnly();
    }
}

// ── Middleware pipeline ───────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Audit middleware runs AFTER auth so we have user identity available,
// but AFTER authorization so we capture the real response status code.
app.UseAuditLogging();

app.MapRazorPages();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
