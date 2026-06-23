using HospitalManagement.Data;
using HospitalManagement.Core.Models;

namespace HospitalManagement.Services;

public class DoctorService
{
    private readonly HospitalDbContext _db;

    public DoctorService(HospitalDbContext db) => _db = db;

    public Doctor AddDoctor(string firstName, string lastName, Specialization specialization,
        string phone, string email, string licenseNumber, decimal consultationFee,
        List<DayOfWeek> availableDays)
    {
        var doctor = new Doctor
        {
            FirstName = firstName, LastName = lastName, Specialization = specialization,
            PhoneNumber = phone, Email = email, LicenseNumber = licenseNumber,
            ConsultationFee = consultationFee, AvailableDays = availableDays,
            WorkStartTime = new TimeSpan(8, 0, 0), WorkEndTime = new TimeSpan(17, 0, 0)
        };
        _db.Doctors.Add(doctor);
        _db.SaveChanges();
        return doctor;
    }

    public Doctor? GetById(int id) => _db.Doctors.Find(id);

    public List<Doctor> GetAll() => _db.Doctors.ToList();

    public List<Doctor> GetBySpecialization(Specialization spec) =>
        _db.Doctors.Where(d => d.Specialization == spec && d.IsAvailable).ToList();

    public List<Doctor> Search(string query)
    {
        query = query.ToLower();
        return _db.Doctors.Where(d =>
            d.FirstName.ToLower().Contains(query) ||
            d.LastName.ToLower().Contains(query)).ToList();
    }

    public bool ToggleAvailability(int id)
    {
        var doctor = GetById(id);
        if (doctor == null) return false;
        doctor.IsAvailable = !doctor.IsAvailable;
        _db.SaveChanges();
        return true;
    }
}
