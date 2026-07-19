using System.ComponentModel.DataAnnotations;
using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Records;

public class RecordFormViewModel
{
    public List<Patient>     Patients     { get; set; } = new();
    public List<Doctor>      Doctors      { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public int  PatientId     { get; set; }
    public int  DoctorId      { get; set; }
    public int? AppointmentId { get; set; }

    [Required] public string Diagnosis     { get; set; } = "";
    [Required] public string Symptoms      { get; set; } = "";
    [Required] public string TreatmentPlan { get; set; } = "";
    public string LabResults { get; set; } = "";
    public string Allergies  { get; set; } = "";
    public string Notes      { get; set; } = "";

    /// <summary>JSON string of Prescription objects, built by the form JS.</summary>
    public string PrescriptionsJson { get; set; } = "[]";
}
