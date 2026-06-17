using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services;

public class MedicalRecordService
{
    private readonly DataStore _store;

    public MedicalRecordService(DataStore store) => _store = store;

    public MedicalRecord CreateRecord(int patientId, int doctorId, int? appointmentId,
        string diagnosis, string symptoms, string treatmentPlan,
        List<Prescription> prescriptions, string labResults = "", string allergies = "", string notes = "")
    {
        var patient = _store.Patients.FirstOrDefault(p => p.Id == patientId);
        var doctor = _store.Doctors.FirstOrDefault(d => d.Id == doctorId);

        var record = new MedicalRecord
        {
            Id = _store.NextRecordId(),
            PatientId = patientId,
            DoctorId = doctorId,
            AppointmentId = appointmentId,
            Diagnosis = diagnosis,
            Symptoms = symptoms,
            TreatmentPlan = treatmentPlan,
            Prescriptions = prescriptions,
            LabResults = labResults,
            Allergies = allergies,
            Notes = notes,
            PatientName = patient?.FullName ?? "Unknown",
            DoctorName = doctor?.FullName ?? "Unknown"
        };

        _store.MedicalRecords.Add(record);

        // Update appointment status if linked
        if (appointmentId.HasValue)
        {
            var appt = _store.Appointments.FirstOrDefault(a => a.Id == appointmentId);
            if (appt != null) appt.Status = AppointmentStatus.Completed;
        }

        return record;
    }

    public MedicalRecord? GetById(int id) =>
        _store.MedicalRecords.FirstOrDefault(r => r.Id == id);

    public List<MedicalRecord> GetByPatient(int patientId) =>
        _store.MedicalRecords
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.RecordDate)
            .ToList();

    public List<MedicalRecord> GetByDoctor(int doctorId) =>
        _store.MedicalRecords.Where(r => r.DoctorId == doctorId).ToList();

    public List<MedicalRecord> GetAll() => _store.MedicalRecords;
}
