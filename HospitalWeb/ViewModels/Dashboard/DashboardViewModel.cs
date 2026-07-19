using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Dashboard;

public class DashboardViewModel
{
    public Patient?            CurrentPatient       { get; set; }
    public List<Appointment>   UpcomingAppointments { get; set; } = new();
    public List<Appointment>   PastAppointments     { get; set; } = new();
    public List<MedicalRecord> RecentRecords        { get; set; } = new();
    public List<Bill>          RecentBills          { get; set; } = new();
    public int                 TotalAppointments    { get; set; }
    public decimal             OutstandingBalance   { get; set; }
}
