using BCrypt.Net;
using HospitalWeb.Models;

namespace HospitalWeb.Data;

public class DbSeeder
{
    private readonly HospitalDbContext _db;

    public DbSeeder(HospitalDbContext db) => _db = db;

    public void Seed()
    {
        // ── Admin accounts (hospital-issued credentials) ──────────────────────
        if (!_db.AppUsers.Any(u => u.Role == UserRole.Admin))
        {
            _db.AppUsers.AddRange(new List<AppUser>
            {
                new() {
                    Username     = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role         = UserRole.Admin,
                    FullName     = "System Administrator",
                    Email        = "admin@cityhospital.co.ug",
                    CreatedAt    = DateTime.UtcNow
                },
                new() {
                    Username     = "reception",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Recep@456"),
                    Role         = UserRole.Admin,
                    FullName     = "Front Desk Reception",
                    Email        = "reception@cityhospital.co.ug",
                    CreatedAt    = DateTime.UtcNow
                }
            });
            _db.SaveChanges();
        }

        // Doctors
        var doctors = new List<Doctor>
        {
            new() { FirstName = "Nakato",   LastName = "Achieng",   Specialization = Specialization.Cardiology,
                    PhoneNumber = "+256 701 234 567", Email = "nakato.achieng@cityhospital.co.ug",
                    LicenseNumber = "UMC-10001", ConsultationFee = 90000, IsAvailable = true,
                    AvailableDays = new() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                    WorkStartTime = new TimeSpan(8,0,0), WorkEndTime = new TimeSpan(17,0,0) },

            new() { FirstName = "Ssemwogerere", LastName = "Kato",  Specialization = Specialization.Neurology,
                    PhoneNumber = "+256 772 345 678", Email = "s.kato@cityhospital.co.ug",
                    LicenseNumber = "UMC-10002", ConsultationFee = 95000, IsAvailable = true,
                    AvailableDays = new() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                    WorkStartTime = new TimeSpan(8,0,0), WorkEndTime = new TimeSpan(17,0,0) },

            new() { FirstName = "Akello",   LastName = "Opio",      Specialization = Specialization.Pediatrics,
                    PhoneNumber = "+256 754 456 789", Email = "akello.opio@cityhospital.co.ug",
                    LicenseNumber = "UMC-10003", ConsultationFee = 60000, IsAvailable = true,
                    AvailableDays = new() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                    WorkStartTime = new TimeSpan(8,0,0), WorkEndTime = new TimeSpan(17,0,0) },

            new() { FirstName = "Mugisha",  LastName = "Tumwesigye", Specialization = Specialization.Surgery,
                    PhoneNumber = "+256 782 567 890", Email = "mugisha.t@cityhospital.co.ug",
                    LicenseNumber = "UMC-10004", ConsultationFee = 100000, IsAvailable = true,
                    AvailableDays = new() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                    WorkStartTime = new TimeSpan(8,0,0), WorkEndTime = new TimeSpan(17,0,0) },

            new() { FirstName = "Nansubuga", LastName = "Nalwoga",  Specialization = Specialization.GeneralPractice,
                    PhoneNumber = "+256 700 678 901", Email = "nansubuga.n@cityhospital.co.ug",
                    LicenseNumber = "UMC-10005", ConsultationFee = 50000, IsAvailable = true,
                    AvailableDays = new() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                    WorkStartTime = new TimeSpan(8,0,0), WorkEndTime = new TimeSpan(17,0,0) },

            new() { FirstName = "Okello",   LastName = "Ongom",     Specialization = Specialization.Orthopedics,
                    PhoneNumber = "+256 753 789 012", Email = "okello.ongom@cityhospital.co.ug",
                    LicenseNumber = "UMC-10006", ConsultationFee = 85000, IsAvailable = true,
                    AvailableDays = new() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                    WorkStartTime = new TimeSpan(8,0,0), WorkEndTime = new TimeSpan(17,0,0) },

            new() { FirstName = "Birungi",  LastName = "Kyomugisha", Specialization = Specialization.Gynecology,
                    PhoneNumber = "+256 774 890 123", Email = "birungi.k@cityhospital.co.ug",
                    LicenseNumber = "UMC-10007", ConsultationFee = 80000, IsAvailable = true,
                    AvailableDays = new() { DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                    WorkStartTime = new TimeSpan(8,0,0), WorkEndTime = new TimeSpan(17,0,0) },
        };
        _db.Doctors.AddRange(doctors);

        // Rooms
        var rooms = new List<Room>
        {
            new() { RoomNumber = "101",    Type = RoomType.General,          Capacity = 4, DailyRate = 50000,  Floor = "1", Ward = "General Ward" },
            new() { RoomNumber = "102",    Type = RoomType.General,          Capacity = 4, DailyRate = 50000,  Floor = "1", Ward = "General Ward" },
            new() { RoomNumber = "201",    Type = RoomType.Private,          Capacity = 1, DailyRate = 75000,  Floor = "2", Ward = "Private Wing" },
            new() { RoomNumber = "202",    Type = RoomType.Private,          Capacity = 1, DailyRate = 75000,  Floor = "2", Ward = "Private Wing" },
            new() { RoomNumber = "ICU-01", Type = RoomType.ICU,              Capacity = 2, DailyRate = 100000, Floor = "3", Ward = "Intensive Care" },
            new() { RoomNumber = "ICU-02", Type = RoomType.ICU,              Capacity = 2, DailyRate = 100000, Floor = "3", Ward = "Intensive Care" },
            new() { RoomNumber = "ER-01",  Type = RoomType.Emergency,        Capacity = 3, DailyRate = 80000,  Floor = "G", Ward = "Emergency" },
            new() { RoomNumber = "OT-01",  Type = RoomType.OperatingTheater, Capacity = 1, DailyRate = 100000, Floor = "3", Ward = "Surgery" },
            new() { RoomNumber = "MAT-01", Type = RoomType.MaternityWard,    Capacity = 2, DailyRate = 65000,  Floor = "2", Ward = "Maternity" },
            new() { RoomNumber = "PED-01", Type = RoomType.PediatricWard,    Capacity = 3, DailyRate = 55000,  Floor = "1", Ward = "Pediatrics" },
        };
        _db.Rooms.AddRange(rooms);

        // Patients
        var patients = new List<Patient>
        {
            new() { FirstName = "Wasswa",   LastName = "Ssemakula",
                    DateOfBirth = new DateTime(1988, 4, 12), Gender = Gender.Male,
                    BloodType = BloodType.OPositive, PhoneNumber = "+256 706 112 233",
                    Email = "wasswa.ssemakula@gmail.com", Address = "Plot 14 Kawempe, Kampala",
                    EmergencyContactName = "Nakato Ssemakula", EmergencyContactPhone = "+256 706 112 244",
                    RegistrationDate = DateTime.Now },

            new() { FirstName = "Aisha",    LastName = "Nabukenya",
                    DateOfBirth = new DateTime(1995, 8, 3),  Gender = Gender.Female,
                    BloodType = BloodType.APositive, PhoneNumber = "+256 772 223 344",
                    Email = "aisha.nabukenya@yahoo.com", Address = "Ntinda, Kampala",
                    EmergencyContactName = "Hamid Nabukenya", EmergencyContactPhone = "+256 772 223 355",
                    RegistrationDate = DateTime.Now },

            new() { FirstName = "Okwir",    LastName = "Odongo",
                    DateOfBirth = new DateTime(1972, 11, 19), Gender = Gender.Male,
                    BloodType = BloodType.BPositive, PhoneNumber = "+256 754 334 455",
                    Email = "okwir.odongo@gmail.com", Address = "Gulu Town, Northern Uganda",
                    EmergencyContactName = "Grace Odongo", EmergencyContactPhone = "+256 754 334 466",
                    RegistrationDate = DateTime.Now },

            new() { FirstName = "Namusoke", LastName = "Nantongo",
                    DateOfBirth = new DateTime(2001, 2, 27), Gender = Gender.Female,
                    BloodType = BloodType.ABPositive, PhoneNumber = "+256 700 445 566",
                    Email = "namusoke.n@outlook.com", Address = "Entebbe Road, Wakiso",
                    EmergencyContactName = "Kizza Nantongo", EmergencyContactPhone = "+256 700 445 577",
                    RegistrationDate = DateTime.Now },

            new() { FirstName = "Tumusiime", LastName = "Rwabwoogo",
                    DateOfBirth = new DateTime(1965, 6, 8),  Gender = Gender.Male,
                    BloodType = BloodType.ONegative, PhoneNumber = "+256 782 556 677",
                    Email = "tumusiime.r@gmail.com", Address = "Mbarara City, Western Uganda",
                    EmergencyContactName = "Agnes Rwabwoogo", EmergencyContactPhone = "+256 782 556 688",
                    RegistrationDate = DateTime.Now },
        };
        _db.Patients.AddRange(patients);
        _db.SaveChanges();
    }

    /// <summary>Ensures admin accounts exist without touching other data.</summary>
    public void SeedAdminsOnly()
    {
        if (!_db.AppUsers.Any(u => u.Role == UserRole.Admin))
        {
            _db.AppUsers.AddRange(new List<AppUser>
            {
                new() {
                    Username     = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role         = UserRole.Admin,
                    FullName     = "System Administrator",
                    Email        = "admin@cityhospital.co.ug",
                    CreatedAt    = DateTime.UtcNow
                },
                new() {
                    Username     = "reception",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Recep@456"),
                    Role         = UserRole.Admin,
                    FullName     = "Front Desk Reception",
                    Email        = "reception@cityhospital.co.ug",
                    CreatedAt    = DateTime.UtcNow
                }
            });
            _db.SaveChanges();
        }
    }
}
