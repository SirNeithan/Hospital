namespace HospitalManagement.Core.Models;

public enum Gender { Male, Female, Other }
public enum BloodType { APositive, ANegative, BPositive, BNegative, ABPositive, ABNegative, OPositive, ONegative, Unknown }

public class Patient
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public DateTime DateOfBirth { get; set; }
    public int Age => (int)((DateTime.Today - DateOfBirth).TotalDays / 365.25);
    public Gender Gender { get; set; }
    public BloodType BloodType { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; } = DateTime.Now;
    public bool IsAdmitted { get; set; } = false;
    public int? AssignedRoomId { get; set; }

    public override string ToString() =>
        $"[{Id}] {FullName} | Age: {Age} | {Gender} | Blood: {BloodType} | Phone: {PhoneNumber}";
}
