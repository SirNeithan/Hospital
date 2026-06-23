using HospitalManagement.Data;
using HospitalManagement.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Services;

public class PatientService
{
    private readonly HospitalDbContext _db;

    public PatientService(HospitalDbContext db) => _db = db;

    public Patient RegisterPatient(string firstName, string lastName, DateTime dob, Gender gender,
        BloodType bloodType, string phone, string email, string address,
        string emergencyName, string emergencyPhone)
    {
        var patient = new Patient
        {
            FirstName = firstName, LastName = lastName, DateOfBirth = dob,
            Gender = gender, BloodType = bloodType, PhoneNumber = phone,
            Email = email, Address = address,
            EmergencyContactName = emergencyName, EmergencyContactPhone = emergencyPhone,
            RegistrationDate = DateTime.Now
        };
        _db.Patients.Add(patient);
        _db.SaveChanges();
        return patient;
    }

    public Patient? GetById(int id) => _db.Patients.Find(id);

    public List<Patient> GetAll() => _db.Patients.OrderByDescending(p => p.RegistrationDate).ToList();

    public List<Patient> Search(string query)
    {
        query = query.ToLower();
        return _db.Patients.Where(p =>
            p.FirstName.ToLower().Contains(query) ||
            p.LastName.ToLower().Contains(query) ||
            p.PhoneNumber.Contains(query) ||
            p.Email.ToLower().Contains(query))
            .ToList();
    }

    public bool UpdatePatient(int id, Action<Patient> updateAction)
    {
        var patient = GetById(id);
        if (patient == null) return false;
        updateAction(patient);
        _db.SaveChanges();
        return true;
    }

    public bool AdmitPatient(int patientId, int roomId)
    {
        var patient = GetById(patientId);
        var room = _db.Rooms.Find(roomId);
        if (patient == null || room == null || patient.IsAdmitted || !room.HasAvailableBed) return false;

        patient.IsAdmitted = true;
        patient.AssignedRoomId = roomId;
        room.CurrentOccupancy++;
        if (room.CurrentOccupancy >= room.Capacity) room.Status = RoomStatus.Occupied;
        _db.SaveChanges();
        return true;
    }

    public bool DischargePatient(int id)
    {
        var patient = GetById(id);
        if (patient == null || !patient.IsAdmitted) return false;

        if (patient.AssignedRoomId.HasValue)
        {
            var room = _db.Rooms.Find(patient.AssignedRoomId.Value);
            if (room != null)
            {
                room.CurrentOccupancy = Math.Max(0, room.CurrentOccupancy - 1);
                if (room.CurrentOccupancy == 0) room.Status = RoomStatus.Available;
            }
        }
        patient.IsAdmitted = false;
        patient.AssignedRoomId = null;
        _db.SaveChanges();
        return true;
    }

    public List<Patient> GetAdmitted() => _db.Patients.Where(p => p.IsAdmitted).ToList();
}
