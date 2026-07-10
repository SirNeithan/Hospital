using HospitalManagement.Data;
using HospitalManagement.Core.Models;

namespace HospitalManagement.Services;

public class MedicalRecordService
{
    private readonly HospitalDbContext _db;

    public MedicalRecordService(HospitalDbContext db) => _db = db;

    public MedicalRecord CreateRecord(int patientId, int doctorId, int? appointmentId,
        string diagnosis, string symptoms, string treatmentPlan,
        List<Prescription> prescriptions, string labResults = "", string allergies = "", string notes = "")
    {
        var patient = _db.Patients.Find(patientId);
        var doctor = _db.Doctors.Find(doctorId);

        var record = new MedicalRecord
        {
            PatientId = patientId, DoctorId = doctorId, AppointmentId = appointmentId,
            Diagnosis = diagnosis, Symptoms = symptoms, TreatmentPlan = treatmentPlan,
            Prescriptions = prescriptions, LabResults = labResults,
            Allergies = allergies, Notes = notes,
            RecordDate = DateTime.Now,
            PatientName = patient?.FullName ?? "Unknown",
            DoctorName = doctor?.FullName ?? "Unknown"
        };
        _db.MedicalRecords.Add(record);

        if (appointmentId.HasValue)
        {
            var appt = _db.Appointments.Find(appointmentId.Value);
            if (appt != null) appt.Status = AppointmentStatus.Completed;
        }

        _db.SaveChanges();
        return record;
    }

    public MedicalRecord? GetById(int id) => _db.MedicalRecords.Find(id);

    public List<MedicalRecord> GetByPatient(int patientId) =>
        _db.MedicalRecords.Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.RecordDate).ToList();

    public List<MedicalRecord> GetAll() =>
        _db.MedicalRecords.OrderByDescending(r => r.RecordDate).ToList();

    /// <summary>Returns a queryable for paginated list views, optionally filtered by patient name.</summary>
    public IQueryable<MedicalRecord> Query(string? search = null)
    {
        var q = _db.MedicalRecords.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            q = q.Where(r => r.PatientName.ToLower().Contains(s) ||
                              r.DoctorName.ToLower().Contains(s)  ||
                              r.Diagnosis.ToLower().Contains(s));
        }
        return q.OrderByDescending(r => r.RecordDate);
    }
}
