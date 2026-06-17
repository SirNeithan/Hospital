namespace HospitalManagementSystem.Models;

public class Prescription
{
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public string Instructions { get; set; } = string.Empty;

    public override string ToString() =>
        $"{MedicineName} {Dosage} - {Frequency} for {DurationDays} days. {Instructions}";
}

public class MedicalRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime RecordDate { get; set; } = DateTime.Now;
    public string Diagnosis { get; set; } = string.Empty;
    public string Symptoms { get; set; } = string.Empty;
    public string TreatmentPlan { get; set; } = string.Empty;
    public List<Prescription> Prescriptions { get; set; } = new();
    public string LabResults { get; set; } = string.Empty;
    public string Allergies { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // Navigation
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;

    public override string ToString() =>
        $"[{Id}] {RecordDate:dd/MM/yyyy} | Patient: {PatientName} | Doctor: {DoctorName} | Diagnosis: {Diagnosis}";
}
