using HospitalWeb.Data;
using HospitalWeb.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

// Tell Npgsql to handle unspecified DateTime as UTC
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// PostgreSQL via EF Core
builder.Services.AddDbContext<HospitalDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cookie-based authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath    = "/Auth/Login";
        options.LogoutPath   = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan   = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly  = true;
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

// Register hospital services as scoped (one per request, uses DbContext)
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<DoctorService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<MedicalRecordService>();
builder.Services.AddScoped<BillingService>();
builder.Services.AddScoped<DataStore>(); // kept for Room access during transition

var app = builder.Build();

// Auto-create DB tables and seed on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HospitalDbContext>();
    db.Database.Migrate();

    // Re-seed: clear old data and insert fresh Ugandan seed data
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
        var seeder = new DbSeeder(db);
        seeder.Seed();
    }
    else
    {
        // Still ensure admin accounts exist even if other data is already seeded
        var seeder = new DbSeeder(db);
        seeder.SeedAdminsOnly();
    }
}

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

app.MapRazorPages();

app.Run();
