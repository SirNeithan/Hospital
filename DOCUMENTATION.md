# Hospital Management System — Technical Documentation

## Overview

The `HospitalWeb` project is an ASP.NET Core web application that manages hospital operations — patients, doctors, appointments, rooms, medical records, and billing. It uses **Razor Pages** (ASP.NET's MVC-flavored page-based model), **Entity Framework Core** as the ORM, and **PostgreSQL** as the database.

---

## 1. MVC Pattern — How It Is Applied

This project uses **Razor Pages**, which is a variation of the MVC (Model-View-Controller) pattern built into ASP.NET Core. Instead of a single controller handling many views, each page owns its own "controller" (called a PageModel). The three roles map like this:

| MVC Role   | In This Project         | Example File                            |
|------------|-------------------------|-----------------------------------------|
| **Model**  | C# model classes        | `Models/Patient.cs`, `Models/Doctor.cs` |
| **View**   | `.cshtml` Razor files   | `Pages/Patients/Index.cshtml`           |
| **Controller** | `.cshtml.cs` PageModel | `Pages/Patients/Index.cshtml.cs`    |

A **Service layer** sits between the PageModel and the database, keeping business logic out of both the view and the data layer.

```
Browser Request
      │
      ▼
  PageModel (.cshtml.cs)   ← handles HTTP GET/POST
      │
      ▼
  Service (PatientService) ← business logic, queries
      │
      ▼
  HospitalDbContext        ← Entity Framework, talks to PostgreSQL
      │
      ▼
  PostgreSQL Database
```

---

### 1.1 The Model

Models are plain C# classes in the `Models/` folder. They define what data exists in the system.

```csharp
// Models/Patient.cs
public class Patient
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";   // computed, not stored in DB
    public DateTime DateOfBirth { get; set; }
    public int Age => (int)((DateTime.Today - DateOfBirth).TotalDays / 365.25);
    public Gender Gender { get; set; }      // enum, stored as string in DB
    public BloodType BloodType { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsAdmitted { get; set; } = false;
    public int? AssignedRoomId { get; set; }  // nullable foreign key
}
```

Key design decisions:
- **Enums** (`Gender`, `BloodType`, `Specialization`, etc.) are stored as their string names in PostgreSQL (e.g. `"Male"`, `"OPositive"`), not as integers. This makes the data readable directly in the database.
- **Computed properties** like `FullName` and `Age` are not stored — they are calculated at runtime using C# property getters.
- **Complex list properties** like `Doctor.AvailableDays` (a `List<DayOfWeek>`) and `Bill.Items` (a `List<BillItem>`) cannot be stored as simple columns and get special treatment (see Section 2.2).

---

### 1.2 The View (.cshtml)

Views are Razor files that combine HTML with C# expressions (`@`). They never talk to the database directly — they only display data provided by the PageModel.

```html
<!-- Pages/Patients/Index.cshtml (simplified) -->
@page
@model HospitalWeb.Pages.Patients.IndexModel

<h2>Patients</h2>

<form method="get">
    <input type="text" name="Search" value="@Model.Search" placeholder="Search…" />
    <button>Search</button>
</form>

@foreach (var p in Model.Patients)
{
    <tr>
        <td>@p.FullName</td>
        <td>@p.Age yrs / @p.Gender</td>
        <td>@p.BloodType</td>
        <td>
            <a href="/Patients/Details?id=@p.Id">View</a>
            <a href="/Patients/Edit?id=@p.Id">Edit</a>
        </td>
    </tr>
}
```

The `@model` directive at the top tells Razor which PageModel this view belongs to. `Model.Patients` and `Model.Search` are properties defined in the PageModel class.

Each section of the application has its own folder under `Pages/`:
```
Pages/
├── Patients/     → Index, Create, Edit, Details, Admit
├── Doctors/      → Index, Create, Edit, Details
├── Appointments/ → Index, Create, Edit, Details
├── Rooms/        → Index, Create, Edit, Details
├── Records/      → Index, Create, Edit, Details
└── Billing/      → Index, Create, Edit, Details
```

---

### 1.3 The PageModel (Controller)

The PageModel is the C# class paired with each view. It handles incoming HTTP requests and prepares data for the view.

```csharp
// Pages/Patients/Index.cshtml.cs
public class IndexModel : PageModel
{
    private readonly PatientService _patients;

    // PatientService is injected via constructor (Dependency Injection)
    public IndexModel(PatientService patients) => _patients = patients;

    // BindProperty makes this auto-fill from the query string (?Search=...)
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    // This list is populated on GET and read by the view
    public List<Patient> Patients { get; set; } = new();

    // OnGet() runs when the browser sends a GET request
    public void OnGet()
    {
        Patients = string.IsNullOrWhiteSpace(Search)
            ? _patients.GetAll()
            : _patients.Search(Search);
    }
}
```

The naming convention `OnGet()` / `OnPost()` is how Razor Pages maps HTTP verbs to methods. No routing attributes are needed — `@page` in the view handles it.

---

### 1.4 The Service Layer

Services contain business logic and all database access. They are injected into PageModels rather than having the PageModel call EF Core directly. This keeps pages thin and logic testable.

```csharp
// Services/PatientService.cs
public class PatientService
{
    private readonly HospitalDbContext _db;

    public PatientService(HospitalDbContext db) => _db = db;

    public List<Patient> GetAll() =>
        _db.Patients.OrderByDescending(p => p.RegistrationDate).ToList();

    public List<Patient> Search(string query)
    {
        query = query.ToLower();
        return _db.Patients.Where(p =>
            p.FirstName.ToLower().Contains(query) ||
            p.LastName.ToLower().Contains(query) ||
            p.PhoneNumber.Contains(query) ||
            p.Email.ToLower().Contains(query))
            .ToList();
    }

    public bool AdmitPatient(int patientId, int roomId)
    {
        var patient = GetById(patientId);
        var room = _db.Rooms.Find(roomId);
        if (patient == null || room == null || patient.IsAdmitted || !room.HasAvailableBed)
            return false;

        patient.IsAdmitted = true;
        patient.AssignedRoomId = roomId;
        room.CurrentOccupancy++;
        _db.SaveChanges();
        return true;
    }
}
```

All six services follow the same pattern:

| Service                | Responsibility                              |
|------------------------|---------------------------------------------|
| `PatientService`       | Register, search, admit, discharge patients |
| `DoctorService`        | Add, update, query doctors by specialization|
| `AppointmentService`   | Book, cancel, complete appointments         |
| `MedicalRecordService` | Create and retrieve patient records         |
| `BillingService`       | Generate bills, record payments             |
| `DataStore`            | Room management (transitional)              |

---

## 2. Entity Framework Core

Entity Framework Core (EF Core) is the ORM (Object-Relational Mapper) that translates between C# objects and PostgreSQL tables. Instead of writing SQL, you work with C# classes and LINQ queries, and EF Core generates the SQL automatically.

---

### 2.1 The DbContext

`HospitalDbContext` is the central EF Core class. It represents the database session and exposes each table as a `DbSet<T>`.

```csharp
// Data/HospitalDbContext.cs
public class HospitalDbContext : DbContext
{
    public HospitalDbContext(DbContextOptions<HospitalDbContext> options) : base(options) { }

    // Each DbSet<T> maps to a database table
    public DbSet<Patient>       Patients       => Set<Patient>();
    public DbSet<Doctor>        Doctors        => Set<Doctor>();
    public DbSet<Appointment>   Appointments   => Set<Appointment>();
    public DbSet<Room>          Rooms          => Set<Room>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Bill>          Bills          => Set<Bill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Additional configuration goes here (see Section 2.2)
    }
}
```

When you write `_db.Patients.Where(...).ToList()`, EF Core translates that LINQ expression into a `SELECT` SQL statement and maps the results back to `Patient` objects.

---

### 2.2 Model Configuration (Fluent API)

`OnModelCreating` is where you fine-tune how C# types map to database columns. The project uses the **Fluent API** style (method chaining on `modelBuilder`).

**String length constraints:**
```csharp
modelBuilder.Entity<Patient>(e =>
{
    e.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
    e.Property(p => p.Email).HasMaxLength(150);
    e.Property(p => p.Address).HasMaxLength(300);
});
```
These become `character varying(100)`, `character varying(150)`, etc. in PostgreSQL.

**Enum → string conversion:**
```csharp
e.Property(p => p.Gender).HasConversion<string>();
e.Property(p => p.BloodType).HasConversion<string>();
```
EF Core stores the enum as its name (e.g. `"Female"`) rather than a number. This makes the database self-documenting.

**List of enums → comma-separated string:**
```csharp
// Doctor.AvailableDays is a List<DayOfWeek>
e.Property(d => d.AvailableDays)
 .HasConversion(
    // C# → DB: convert list to "1,3,5"
    v => string.Join(',', v.Select(x => (int)x)),
    // DB → C#: parse "1,3,5" back to List<DayOfWeek>
    v => string.IsNullOrEmpty(v)
        ? new List<DayOfWeek>()
        : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
           .Select(x => (DayOfWeek)int.Parse(x)).ToList()
 );
```
A `List<DayOfWeek>` can't map to a single column by default, so a custom converter serialises it to/from a comma-separated string.

**Complex objects → JSON columns (jsonb):**
```csharp
// MedicalRecord.Prescriptions is a List<Prescription>
e.Property(r => r.Prescriptions).HasColumnType("jsonb");

// Bill.Items is a List<BillItem>
e.Property(b => b.Items).HasColumnType("jsonb");
```
PostgreSQL's `jsonb` type stores structured data natively. EF Core + Npgsql automatically serialise the C# list to JSON when saving and deserialise back when reading — no extra code needed.

---

### 2.3 Registering the DbContext

In `Program.cs`, the DbContext is registered in the dependency injection container so services and pages can receive it automatically:

```csharp
builder.Services.AddDbContext<HospitalDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

The connection string lives in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=hospital_db;Username=...;Password=..."
  }
}
```

Services are also registered here as **Scoped** — meaning one instance per HTTP request, which matches the lifetime of `HospitalDbContext`:

```csharp
builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<DoctorService>();
builder.Services.AddScoped<AppointmentService>();
// ... and so on
```

---

## 3. Migrations

Migrations are EF Core's way of keeping the database schema in sync with C# model changes. Instead of writing `CREATE TABLE` SQL by hand, you generate and apply migration files that describe the changes.

---

### 3.1 How Migrations Work — The Concept

```
1. You change a model class (add a property, rename something, etc.)
        │
        ▼
2. Run:  dotnet ef migrations add <MigrationName>
        │
        ▼
3. EF Core compares your models against the last known snapshot
   and generates a new migration file (Up + Down methods)
        │
        ▼
4. Run:  dotnet ef database update
        │
        ▼
5. EF Core executes the migration SQL against the real database
```

Every migration has two methods:
- **`Up()`** — what to do when applying (e.g. `CreateTable`, `AddColumn`)
- **`Down()`** — how to undo it (e.g. `DropTable`, `DropColumn`)

---

### 3.2 The InitialCreate Migration

This project has one migration: `20260616192456_InitialCreate`. The filename format is `<timestamp>_<name>`, which is how EF Core orders migrations.

The `Up()` method creates all six tables from scratch:

```csharp
// Migrations/20260616192456_InitialCreate.cs (excerpt)
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "Patients",
        columns: table => new
        {
            Id = table.Column<int>(type: "integer", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy",
                             NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            FirstName  = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
            LastName   = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
            Gender     = table.Column<string>(type: "text", nullable: false),  // stored as string
            BloodType  = table.Column<string>(type: "text", nullable: false),
            IsAdmitted = table.Column<bool>(type: "boolean", nullable: false),
            AssignedRoomId = table.Column<int>(type: "integer", nullable: true),  // nullable FK
            // ... more columns
        },
        constraints: table => table.PrimaryKey("PK_Patients", x => x.Id)
    );

    migrationBuilder.CreateTable(
        name: "MedicalRecords",
        columns: table => new
        {
            // ...
            Prescriptions = table.Column<List<Prescription>>(type: "jsonb", nullable: false),
        },
        // ...
    );

    // ... Bills, Doctors, Appointments, Rooms tables
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropTable(name: "Appointments");
    migrationBuilder.DropTable(name: "Bills");
    migrationBuilder.DropTable(name: "Doctors");
    migrationBuilder.DropTable(name: "MedicalRecords");
    migrationBuilder.DropTable(name: "Patients");
    migrationBuilder.DropTable(name: "Rooms");
}
```

Notable things in the generated SQL mapping:
| C# Type         | PostgreSQL Column Type         | Reason                              |
|-----------------|-------------------------------|-------------------------------------|
| `int` (PK)      | `integer` + identity          | Auto-incrementing primary key       |
| `string` (enum) | `text`                        | Stored as enum name string          |
| `string` (max)  | `character varying(N)`        | `HasMaxLength(N)` in config         |
| `DateTime`      | `timestamp with time zone`    | Npgsql default for DateTime         |
| `TimeSpan`      | `interval`                    | Npgsql mapping for time durations   |
| `List<T>`       | `jsonb`                       | `HasColumnType("jsonb")` in config  |
| `bool`          | `boolean`                     | Direct mapping                      |
| `decimal`       | `numeric`                     | Precise decimal for money/rates     |

---

### 3.3 The Model Snapshot

Alongside every migration lives `HospitalDbContextModelSnapshot.cs`. EF Core maintains this file automatically — it represents the **current known state** of the model. When you add a new migration, EF Core diffs your current models against this snapshot to know what changed. You never edit this file manually.

---

### 3.4 Auto-Applying Migrations at Startup

Rather than requiring a manual `dotnet ef database update` command in production, the app applies migrations automatically when it starts:

```csharp
// Program.cs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HospitalDbContext>();
    db.Database.Migrate();  // creates tables if they don't exist, applies pending migrations
    
    if (!db.Doctors.Any())
    {
        var seeder = new DbSeeder(db);
        seeder.Seed();   // insert initial data
    }
}
```

`db.Database.Migrate()` is safe to call repeatedly — it only applies migrations that haven't run yet, tracked in a `__EFMigrationsHistory` table in the database.

---

### 3.5 The DbSeeder

After migrations ensure the schema exists, `DbSeeder` inserts initial data if the tables are empty:

```csharp
// Data/DbSeeder.cs
public class DbSeeder
{
    private readonly HospitalDbContext _db;
    public DbSeeder(HospitalDbContext db) => _db = db;

    public void Seed()
    {
        // Add 7 doctors with Ugandan names and realistic data
        var doctors = new List<Doctor> { /* ... */ };
        _db.Doctors.AddRange(doctors);

        // Add 10 rooms (General, Private, ICU, Emergency, etc.)
        var rooms = new List<Room> { /* ... */ };
        _db.Rooms.AddRange(rooms);

        // Add 5 patients with Ugandan names and contact details
        var patients = new List<Patient> { /* ... */ };
        _db.Patients.AddRange(patients);

        _db.SaveChanges(); // single transaction, inserts all at once
    }
}
```

---

## 4. Full Request Lifecycle — Example

Here is what happens when a user visits `/Patients` in the browser:

```
1. Browser → GET /Patients
   └─ ASP.NET Core routing matches to Pages/Patients/Index.cshtml

2. IndexModel.OnGet() is called
   └─ PatientService is injected via constructor DI
   └─ Calls _patients.GetAll() or _patients.Search(query)

3. PatientService.GetAll() runs
   └─ _db.Patients.OrderByDescending(p => p.RegistrationDate).ToList()
   └─ EF Core generates: SELECT * FROM "Patients" ORDER BY "RegistrationDate" DESC
   └─ PostgreSQL executes the query
   └─ EF Core maps rows → List<Patient> objects

4. IndexModel.Patients property is populated

5. Razor engine renders Index.cshtml
   └─ @foreach (var p in Model.Patients) loops over the list
   └─ HTML with patient data is built

6. Browser ← HTML response
```

---

## 5. Project Structure Summary

```
HospitalWeb/
├── Models/                  ← Plain C# classes (the M in MVC)
│   ├── Patient.cs
│   ├── Doctor.cs
│   ├── Appointment.cs
│   ├── Room.cs
│   ├── MedicalRecord.cs
│   └── Bill.cs
│
├── Data/
│   ├── HospitalDbContext.cs ← EF Core context, table config (Fluent API)
│   └── DbSeeder.cs          ← Initial data seeding
│
├── Services/                ← Business logic + database queries
│   ├── PatientService.cs
│   ├── DoctorService.cs
│   ├── AppointmentService.cs
│   ├── MedicalRecordService.cs
│   ├── BillingService.cs
│   └── DataStore.cs
│
├── Pages/                   ← Views (.cshtml) + PageModels (.cshtml.cs)
│   ├── Patients/            ← Index, Create, Edit, Details, Admit
│   ├── Doctors/
│   ├── Appointments/
│   ├── Rooms/
│   ├── Records/
│   └── Billing/
│
├── Migrations/              ← EF Core generated migration files
│   ├── 20260616192456_InitialCreate.cs
│   └── HospitalDbContextModelSnapshot.cs
│
├── appsettings.json         ← DB connection string
└── Program.cs               ← App setup, DI registration, migration runner
```

---

## 6. Key Commands Reference

```bash
# Create a new migration after changing a model
dotnet ef migrations add <DescriptiveName>

# Apply pending migrations to the database
dotnet ef database update

# Roll back to a specific migration
dotnet ef database update <MigrationName>

# Remove the last migration (if not yet applied)
dotnet ef migrations remove

# See migration history
dotnet ef migrations list
```
