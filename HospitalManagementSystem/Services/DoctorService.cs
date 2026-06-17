using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services;

public class DoctorService
{
    private readonly DataStore _store;

    public DoctorService(DataStore store) => _store = store;

    public Doctor AddDoctor(string firstName, string lastName, Specialization specialization,
        string phone, string email, string licenseNumber, decimal consultationFee,
        List<DayOfWeek> availableDays)
    {
        var doctor = new Doctor
        {
            Id = _store.NextDoctorId(),
            FirstName = firstName,
            LastName = lastName,
            Specialization = specialization,
            PhoneNumber = phone,
            Email = email,
            LicenseNumber = licenseNumber,
            ConsultationFee = consultationFee,
            AvailableDays = availableDays
        };
        _store.Doctors.Add(doctor);
        return doctor;
    }

    public Doctor? GetById(int id) =>
        _store.Doctors.FirstOrDefault(d => d.Id == id);

    public List<Doctor> GetAll() => _store.Doctors;

    public List<Doctor> GetBySpecialization(Specialization spec) =>
        _store.Doctors.Where(d => d.Specialization == spec && d.IsAvailable).ToList();

    public List<Doctor> Search(string query)
    {
        query = query.ToLower();
        return _store.Doctors
            .Where(d => d.FullName.ToLower().Contains(query)
                     || d.Specialization.ToString().ToLower().Contains(query)
                     || d.Id.ToString() == query)
            .ToList();
    }

    public List<Doctor> GetAvailableOnDay(DayOfWeek day) =>
        _store.Doctors.Where(d => d.IsAvailable && d.AvailableDays.Contains(day)).ToList();

    public bool ToggleAvailability(int id)
    {
        var doctor = GetById(id);
        if (doctor == null) return false;
        doctor.IsAvailable = !doctor.IsAvailable;
        return true;
    }
}
