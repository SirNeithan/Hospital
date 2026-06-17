using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services;

public class AppointmentService
{
    private readonly DataStore _store;

    public AppointmentService(DataStore store) => _store = store;

    public (bool Success, string Message, Appointment? Appointment) ScheduleAppointment(
        int patientId, int doctorId, DateTime appointmentDate, string reason, string notes = "")
    {
        var patient = _store.Patients.FirstOrDefault(p => p.Id == patientId);
        if (patient == null) return (false, "Patient not found.", null);

        var doctor = _store.Doctors.FirstOrDefault(d => d.Id == doctorId);
        if (doctor == null) return (false, "Doctor not found.", null);

        if (!doctor.IsAvailable)
            return (false, $"{doctor.FullName} is currently unavailable.", null);

        if (!doctor.AvailableDays.Contains(appointmentDate.DayOfWeek))
            return (false, $"{doctor.FullName} does not work on {appointmentDate.DayOfWeek}.", null);

        // Check for conflicts (same doctor, overlapping 30-min slots)
        bool conflict = _store.Appointments.Any(a =>
            a.DoctorId == doctorId &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.Completed &&
            Math.Abs((a.AppointmentDate - appointmentDate).TotalMinutes) < 30);

        if (conflict)
            return (false, "That time slot is already booked for this doctor.", null);

        var appointment = new Appointment
        {
            Id = _store.NextAppointmentId(),
            PatientId = patientId,
            DoctorId = doctorId,
            AppointmentDate = appointmentDate,
            Reason = reason,
            Notes = notes,
            PatientName = patient.FullName,
            DoctorName = doctor.FullName
        };

        _store.Appointments.Add(appointment);
        return (true, "Appointment scheduled successfully.", appointment);
    }

    public Appointment? GetById(int id) =>
        _store.Appointments.FirstOrDefault(a => a.Id == id);

    public List<Appointment> GetAll() => _store.Appointments;

    public List<Appointment> GetByPatient(int patientId) =>
        _store.Appointments.Where(a => a.PatientId == patientId).OrderByDescending(a => a.AppointmentDate).ToList();

    public List<Appointment> GetByDoctor(int doctorId) =>
        _store.Appointments.Where(a => a.DoctorId == doctorId).OrderBy(a => a.AppointmentDate).ToList();

    public List<Appointment> GetToday() =>
        _store.Appointments
            .Where(a => a.AppointmentDate.Date == DateTime.Today &&
                        a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.AppointmentDate)
            .ToList();

    public List<Appointment> GetUpcoming(int days = 7) =>
        _store.Appointments
            .Where(a => a.AppointmentDate >= DateTime.Now &&
                        a.AppointmentDate <= DateTime.Now.AddDays(days) &&
                        a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.AppointmentDate)
            .ToList();

    public bool UpdateStatus(int id, AppointmentStatus status)
    {
        var appt = GetById(id);
        if (appt == null) return false;
        appt.Status = status;
        return true;
    }

    public bool Cancel(int id)
    {
        var appt = GetById(id);
        if (appt == null || appt.Status == AppointmentStatus.Completed) return false;
        appt.Status = AppointmentStatus.Cancelled;
        return true;
    }
}
