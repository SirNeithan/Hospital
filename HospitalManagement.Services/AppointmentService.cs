using HospitalManagement.Data;
using HospitalManagement.Core.Models;

namespace HospitalManagement.Services;

public class AppointmentService
{
    private readonly HospitalDbContext _db;

    public AppointmentService(HospitalDbContext db) => _db = db;

    public (bool Success, string Message, Appointment? Appointment) ScheduleAppointment(
        int patientId, int doctorId, DateTime appointmentDate, string reason, string notes = "")
    {
        var patient = _db.Patients.Find(patientId);
        if (patient == null) return (false, "Patient not found.", null);

        var doctor = _db.Doctors.Find(doctorId);
        if (doctor == null) return (false, "Doctor not found.", null);

        if (!doctor.IsAvailable) return (false, $"{doctor.FullName} is currently unavailable.", null);

        // Normalise to unspecified kind so DayOfWeek is always local calendar day
        var localDate = DateTime.SpecifyKind(appointmentDate, DateTimeKind.Unspecified);

        if (!doctor.AvailableDays.Contains(localDate.DayOfWeek))
            return (false, $"{doctor.FullName} does not work on {localDate.DayOfWeek}s. " +
                           $"Available: {string.Join(", ", doctor.AvailableDays)}.", null);

        // Check appointment is within doctor's working hours
        if (localDate.TimeOfDay < doctor.WorkStartTime || localDate.TimeOfDay >= doctor.WorkEndTime)
            return (false, $"{doctor.FullName} works {doctor.WorkStartTime:hh\\:mm}–{doctor.WorkEndTime:hh\\:mm}. " +
                           $"Please choose a time within those hours.", null);

        // Check 30-minute slot conflict
        bool conflict = _db.Appointments.AsEnumerable().Any(a =>
            a.DoctorId == doctorId &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.Completed &&
            Math.Abs((a.AppointmentDate - localDate).TotalMinutes) < 30);

        if (conflict) return (false, "That time slot is already booked for this doctor. Please choose a different time.", null);

        var appointment = new Appointment
        {
            PatientId = patientId, DoctorId = doctorId,
            AppointmentDate = localDate, Reason = reason, Notes = notes,
            PatientName = patient.FullName, DoctorName = doctor.FullName,
            CreatedAt = DateTime.Now
        };
        _db.Appointments.Add(appointment);
        _db.SaveChanges();
        return (true, "Appointment scheduled successfully.", appointment);
    }

    public Appointment? GetById(int id) => _db.Appointments.Find(id);

    public List<Appointment> GetAll() => _db.Appointments.ToList();

    /// <summary>Returns a queryable filtered by optional status/date range for paginated lists.</summary>
    public IQueryable<Appointment> Query(string? filter = null)
    {
        var today    = DateTime.Today;
        var tomorrow = today.AddDays(1);
        IQueryable<Appointment> q = filter switch
        {
            "today"    => _db.Appointments.Where(a =>
                              a.AppointmentDate >= today &&
                              a.AppointmentDate < tomorrow &&
                              a.Status != AppointmentStatus.Cancelled),
            "upcoming" => _db.Appointments.Where(a =>
                              a.AppointmentDate >= DateTime.Now &&
                              a.Status != AppointmentStatus.Cancelled),
            _          => _db.Appointments
        };
        return q.OrderByDescending(a => a.AppointmentDate);
    }

    public List<Appointment> GetByPatient(int patientId) =>
        _db.Appointments.Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AppointmentDate).ToList();

    public List<Appointment> GetByDoctor(int doctorId) =>
        _db.Appointments.Where(a => a.DoctorId == doctorId)
            .OrderBy(a => a.AppointmentDate).ToList();

    public List<Appointment> GetToday()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        return _db.Appointments.Where(a =>
            a.AppointmentDate >= today &&
            a.AppointmentDate < tomorrow &&
            a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.AppointmentDate).ToList();
    }

    public List<Appointment> GetUpcoming(int days = 7) =>
        _db.Appointments.Where(a =>
            a.AppointmentDate >= DateTime.Now &&
            a.AppointmentDate <= DateTime.Now.AddDays(days) &&
            a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.AppointmentDate).ToList();

    public bool UpdateStatus(int id, AppointmentStatus status)
    {
        var appt = GetById(id);
        if (appt == null) return false;
        appt.Status = status;
        _db.SaveChanges();
        return true;
    }

    public bool Cancel(int id)
    {
        var appt = GetById(id);
        if (appt == null || appt.Status == AppointmentStatus.Completed) return false;
        appt.Status = AppointmentStatus.Cancelled;
        _db.SaveChanges();
        return true;
    }
}
