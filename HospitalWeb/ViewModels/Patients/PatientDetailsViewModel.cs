using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Patients;

public class PatientDetailsViewModel
{
    public Patient Patient { get; set; } = null!;
    public Room? AssignedRoom { get; set; }
    public List<Appointment>   Appointments { get; set; } = new();
    public List<MedicalRecord> Records      { get; set; } = new();
    public List<Bill>          Bills        { get; set; } = new();
}
