using HospitalManagement.Data;
using HospitalManagement.Core.Models;

namespace HospitalManagement.Services;

/// <summary>
/// Thin wrapper around DbContext for pages that reference DataStore directly (e.g. Rooms).
/// </summary>
public class DataStore
{
    private readonly HospitalDbContext _db;

    public DataStore(HospitalDbContext db) => _db = db;

    public List<Room> Rooms => _db.Rooms.ToList();
    public List<Patient> Patients => _db.Patients.ToList();
    public List<Doctor> Doctors => _db.Doctors.ToList();
    public List<Appointment> Appointments => _db.Appointments.ToList();
    public List<MedicalRecord> MedicalRecords => _db.MedicalRecords.ToList();
    public List<Bill> Bills => _db.Bills.ToList();

    public Room? GetRoom(int id) => _db.Rooms.Find(id);

    public void UpdateRoom(int id, RoomStatus status)
    {
        var room = _db.Rooms.Find(id);
        if (room != null) { room.Status = status; _db.SaveChanges(); }
    }

    public void AddRoom(Room room) { _db.Rooms.Add(room); _db.SaveChanges(); }

    public int NextRoomId() => 0; // EF handles IDs automatically
}
