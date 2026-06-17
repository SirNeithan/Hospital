using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services;

public class PatientService
{
    private readonly DataStore _store;

    public PatientService(DataStore store) => _store = store;

    public Patient RegisterPatient(string firstName, string lastName, DateTime dob, Gender gender,
        BloodType bloodType, string phone, string email, string address,
        string emergencyName, string emergencyPhone)
    {
        var patient = new Patient
        {
            Id = _store.NextPatientId(),
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dob,
            Gender = gender,
            BloodType = bloodType,
            PhoneNumber = phone,
            Email = email,
            Address = address,
            EmergencyContactName = emergencyName,
            EmergencyContactPhone = emergencyPhone
        };
        _store.Patients.Add(patient);
        return patient;
    }

    public Patient? GetById(int id) =>
        _store.Patients.FirstOrDefault(p => p.Id == id);

    public List<Patient> GetAll() => _store.Patients;

    public List<Patient> Search(string query)
    {
        query = query.ToLower();
        return _store.Patients
            .Where(p => p.FullName.ToLower().Contains(query)
                     || p.PhoneNumber.Contains(query)
                     || p.Email.ToLower().Contains(query)
                     || p.Id.ToString() == query)
            .ToList();
    }

    public bool UpdatePatient(int id, Action<Patient> updateAction)
    {
        var patient = GetById(id);
        if (patient == null) return false;
        updateAction(patient);
        return true;
    }

    public bool DischargePatient(int id)
    {
        var patient = GetById(id);
        if (patient == null || !patient.IsAdmitted) return false;

        if (patient.AssignedRoomId.HasValue)
        {
            var room = _store.Rooms.FirstOrDefault(r => r.Id == patient.AssignedRoomId);
            if (room != null)
            {
                room.CurrentOccupancy = Math.Max(0, room.CurrentOccupancy - 1);
                if (room.CurrentOccupancy == 0)
                    room.Status = RoomStatus.Available;
            }
        }

        patient.IsAdmitted = false;
        patient.AssignedRoomId = null;
        return true;
    }

    public bool AdmitPatient(int patientId, int roomId)
    {
        var patient = GetById(patientId);
        var room = _store.Rooms.FirstOrDefault(r => r.Id == roomId);

        if (patient == null || room == null || patient.IsAdmitted || !room.HasAvailableBed)
            return false;

        patient.IsAdmitted = true;
        patient.AssignedRoomId = roomId;
        room.CurrentOccupancy++;
        if (room.CurrentOccupancy >= room.Capacity)
            room.Status = RoomStatus.Occupied;

        return true;
    }

    public List<Patient> GetAdmitted() =>
        _store.Patients.Where(p => p.IsAdmitted).ToList();
}
