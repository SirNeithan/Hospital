using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services;

/// <summary>
/// In-memory data store acting as a simple repository for all entities.
/// </summary>
public class DataStore
{
    private int _patientIdCounter = 1;
    private int _doctorIdCounter = 1;
    private int _appointmentIdCounter = 1;
    private int _roomIdCounter = 1;
    private int _recordIdCounter = 1;
    private int _billIdCounter = 1;

    public List<Patient> Patients { get; } = new();
    public List<Doctor> Doctors { get; } = new();
    public List<Appointment> Appointments { get; } = new();
    public List<Room> Rooms { get; } = new();
    public List<MedicalRecord> MedicalRecords { get; } = new();
    public List<Bill> Bills { get; } = new();

    public int NextPatientId() => _patientIdCounter++;
    public int NextDoctorId() => _doctorIdCounter++;
    public int NextAppointmentId() => _appointmentIdCounter++;
    public int NextRoomId() => _roomIdCounter++;
    public int NextRecordId() => _recordIdCounter++;
    public int NextBillId() => _billIdCounter++;

    public void SeedData()
    {
        // Seed doctors
        var doctors = new List<Doctor>
        {
            new() { Id = NextDoctorId(), FirstName = "Alice", LastName = "Thompson", Specialization = Specialization.Cardiology,
                    PhoneNumber = "555-1001", Email = "a.thompson@hospital.com", LicenseNumber = "LIC-10001",
                    ConsultationFee = 200, AvailableDays = new() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday } },
            new() { Id = NextDoctorId(), FirstName = "Brian", LastName = "Nguyen", Specialization = Specialization.Neurology,
                    PhoneNumber = "555-1002", Email = "b.nguyen@hospital.com", LicenseNumber = "LIC-10002",
                    ConsultationFee = 250, AvailableDays = new() { DayOfWeek.Tuesday, DayOfWeek.Thursday } },
            new() { Id = NextDoctorId(), FirstName = "Carmen", LastName = "Rodriguez", Specialization = Specialization.Pediatrics,
                    PhoneNumber = "555-1003", Email = "c.rodriguez@hospital.com", LicenseNumber = "LIC-10003",
                    ConsultationFee = 150, AvailableDays = new() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday } },
            new() { Id = NextDoctorId(), FirstName = "David", LastName = "Kim", Specialization = Specialization.Surgery,
                    PhoneNumber = "555-1004", Email = "d.kim@hospital.com", LicenseNumber = "LIC-10004",
                    ConsultationFee = 350, AvailableDays = new() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday } },
            new() { Id = NextDoctorId(), FirstName = "Elena", LastName = "Patel", Specialization = Specialization.GeneralPractice,
                    PhoneNumber = "555-1005", Email = "e.patel@hospital.com", LicenseNumber = "LIC-10005",
                    ConsultationFee = 100, AvailableDays = new() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday } },
        };
        Doctors.AddRange(doctors);

        // Seed rooms
        var rooms = new List<Room>
        {
            new() { Id = NextRoomId(), RoomNumber = "101", Type = RoomType.General, Capacity = 4, DailyRate = 150, Floor = "1", Ward = "General Ward" },
            new() { Id = NextRoomId(), RoomNumber = "102", Type = RoomType.General, Capacity = 4, DailyRate = 150, Floor = "1", Ward = "General Ward" },
            new() { Id = NextRoomId(), RoomNumber = "201", Type = RoomType.Private, Capacity = 1, DailyRate = 300, Floor = "2", Ward = "Private Wing" },
            new() { Id = NextRoomId(), RoomNumber = "202", Type = RoomType.Private, Capacity = 1, DailyRate = 300, Floor = "2", Ward = "Private Wing" },
            new() { Id = NextRoomId(), RoomNumber = "ICU-01", Type = RoomType.ICU, Capacity = 2, DailyRate = 800, Floor = "3", Ward = "Intensive Care" },
            new() { Id = NextRoomId(), RoomNumber = "ICU-02", Type = RoomType.ICU, Capacity = 2, DailyRate = 800, Floor = "3", Ward = "Intensive Care" },
            new() { Id = NextRoomId(), RoomNumber = "ER-01", Type = RoomType.Emergency, Capacity = 3, DailyRate = 500, Floor = "G", Ward = "Emergency" },
            new() { Id = NextRoomId(), RoomNumber = "OT-01", Type = RoomType.OperatingTheater, Capacity = 1, DailyRate = 1200, Floor = "3", Ward = "Surgery" },
            new() { Id = NextRoomId(), RoomNumber = "MAT-01", Type = RoomType.MaternityWard, Capacity = 2, DailyRate = 350, Floor = "2", Ward = "Maternity" },
            new() { Id = NextRoomId(), RoomNumber = "PED-01", Type = RoomType.PediatricWard, Capacity = 3, DailyRate = 250, Floor = "1", Ward = "Pediatrics" },
        };
        Rooms.AddRange(rooms);

        // Seed patients
        var patients = new List<Patient>
        {
            new() { Id = NextPatientId(), FirstName = "John", LastName = "Smith", DateOfBirth = new DateTime(1985, 3, 15),
                    Gender = Gender.Male, BloodType = BloodType.APositive, PhoneNumber = "555-2001",
                    Email = "john.smith@email.com", Address = "123 Oak St", EmergencyContactName = "Mary Smith", EmergencyContactPhone = "555-2002" },
            new() { Id = NextPatientId(), FirstName = "Sarah", LastName = "Johnson", DateOfBirth = new DateTime(1992, 7, 22),
                    Gender = Gender.Female, BloodType = BloodType.BNegative, PhoneNumber = "555-2003",
                    Email = "sarah.j@email.com", Address = "456 Maple Ave", EmergencyContactName = "Tom Johnson", EmergencyContactPhone = "555-2004" },
            new() { Id = NextPatientId(), FirstName = "Michael", LastName = "Davis", DateOfBirth = new DateTime(1970, 11, 5),
                    Gender = Gender.Male, BloodType = BloodType.OPositive, PhoneNumber = "555-2005",
                    Email = "m.davis@email.com", Address = "789 Pine Rd", EmergencyContactName = "Linda Davis", EmergencyContactPhone = "555-2006" },
        };
        Patients.AddRange(patients);

        // Seed appointments
        var appt1 = new Appointment
        {
            Id = NextAppointmentId(), PatientId = 1, DoctorId = 1,
            AppointmentDate = DateTime.Today.AddDays(1).AddHours(10),
            Status = AppointmentStatus.Confirmed, Reason = "Chest pain follow-up",
            PatientName = "John Smith", DoctorName = "Dr. Alice Thompson"
        };
        var appt2 = new Appointment
        {
            Id = NextAppointmentId(), PatientId = 2, DoctorId = 5,
            AppointmentDate = DateTime.Today.AddDays(2).AddHours(14),
            Status = AppointmentStatus.Scheduled, Reason = "Annual checkup",
            PatientName = "Sarah Johnson", DoctorName = "Dr. Elena Patel"
        };
        Appointments.AddRange(new[] { appt1, appt2 });
    }
}
