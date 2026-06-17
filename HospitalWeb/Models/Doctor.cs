namespace HospitalWeb.Models;

public enum Specialization
{
    GeneralPractice,
    Cardiology,
    Neurology,
    Orthopedics,
    Pediatrics,
    Oncology,
    Dermatology,
    Gynecology,
    Psychiatry,
    Radiology,
    Surgery,
    Emergency
}

public class Doctor
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"Dr. {FirstName} {LastName}";
    public Specialization Specialization { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
    public List<DayOfWeek> AvailableDays { get; set; } = new();
    public TimeSpan WorkStartTime { get; set; } = new TimeSpan(8, 0, 0);
    public TimeSpan WorkEndTime { get; set; } = new TimeSpan(17, 0, 0);
    public bool IsAvailable { get; set; } = true;

    public override string ToString() =>
        $"[{Id}] {FullName} | {Specialization} | Fee: UGX {ConsultationFee:F2} | Phone: {PhoneNumber}";
}
