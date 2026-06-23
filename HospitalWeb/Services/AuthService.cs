using HospitalWeb.Data;
using HospitalWeb.Models;
using BCrypt.Net;

namespace HospitalWeb.Services;

public class AuthService
{
    private readonly HospitalDbContext _db;

    public AuthService(HospitalDbContext db) => _db = db;

    /// <summary>Creates an admin account. Only admins can create other admins.</summary>
    public (bool Success, string Message, AppUser? User) CreateAdmin(
        string username, string password, string fullName, string email)
    {
        if (_db.AppUsers.Any(u => u.Username == username))
            return (false, "Username already exists.", null);

        var user = new AppUser
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Admin,
            FullName = fullName,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
        _db.AppUsers.Add(user);
        _db.SaveChanges();
        return (true, "Admin account created.", user);
    }

    /// <summary>Registers a patient user account, also creating the Patient record.</summary>
    public (bool Success, string Message, AppUser? User) RegisterPatient(
        string username, string password,
        string firstName, string lastName, DateTime dob,
        Gender gender, BloodType bloodType,
        string phone, string email, string address,
        string emergencyName, string emergencyPhone)
    {
        if (_db.AppUsers.Any(u => u.Username == username))
            return (false, "Username is already taken.", null);

        // Create the Patient record first
        var patient = new Patient
        {
            FirstName = firstName, LastName = lastName, DateOfBirth = dob,
            Gender = gender, BloodType = bloodType, PhoneNumber = phone,
            Email = email, Address = address,
            EmergencyContactName = emergencyName, EmergencyContactPhone = emergencyPhone,
            RegistrationDate = DateTime.Now
        };
        _db.Patients.Add(patient);
        _db.SaveChanges(); // get patient.Id

        // Create the user account linked to that patient
        var user = new AppUser
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Patient,
            PatientId = patient.Id,
            FullName = patient.FullName,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
        _db.AppUsers.Add(user);
        _db.SaveChanges();
        return (true, "Registration successful.", user);
    }

    /// <summary>Validates credentials and returns the user if correct.</summary>
    public AppUser? Login(string username, string password)
    {
        var user = _db.AppUsers.FirstOrDefault(u =>
            u.Username == username && u.IsActive);

        if (user == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

        user.LastLogin = DateTime.UtcNow;
        _db.SaveChanges();
        return user;
    }

    public AppUser? GetById(int id) => _db.AppUsers.Find(id);

    public List<AppUser> GetAll() => _db.AppUsers.OrderBy(u => u.Role).ThenBy(u => u.Username).ToList();

    public bool ToggleActive(int id)
    {
        var user = _db.AppUsers.Find(id);
        if (user == null) return false;
        user.IsActive = !user.IsActive;
        _db.SaveChanges();
        return true;
    }

    public (bool Success, string Message) ChangePassword(int userId, string currentPassword, string newPassword)
    {
        var user = _db.AppUsers.Find(userId);
        if (user == null) return (false, "User not found.");
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            return (false, "Current password is incorrect.");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _db.SaveChanges();
        return (true, "Password changed successfully.");
    }
}
