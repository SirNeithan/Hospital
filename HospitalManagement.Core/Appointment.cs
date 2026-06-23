namespace HospitalManagement.Core.Models;

public enum AppointmentStatus
{
    Scheduled,
    Confirmed,
    InProgress,
    Completed,
    Cancelled,
    NoShow
}

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(30);
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string Reason { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation (resolved at display time)
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;

    public override string ToString() =>
        $"[{Id}] {AppointmentDate:dd/MM/yyyy HH:mm} | Patient: {PatientName} | Doctor: {DoctorName} | {Status} | Reason: {Reason}";
}
