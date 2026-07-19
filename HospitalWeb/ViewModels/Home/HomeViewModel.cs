using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Home;

/// <summary>ViewModel for the admin dashboard (home page).</summary>
public class HomeViewModel
{
    public int     TotalPatients         { get; set; }
    public int     AdmittedPatients      { get; set; }
    public int     TotalDoctors          { get; set; }
    public int     AvailableDoctors      { get; set; }
    public int     TodayAppointments     { get; set; }
    public int     TotalRooms            { get; set; }
    public int     AvailableRooms        { get; set; }
    public decimal TotalRevenue          { get; set; }
    public decimal Outstanding           { get; set; }
    public int     UnpaidBills           { get; set; }
    public List<Appointment> UpcomingAppointments { get; set; } = new();
    public List<Patient>     RecentPatients       { get; set; } = new();
}
