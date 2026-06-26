using HospitalManagement.Core;
using HospitalManagement.Data;
using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Dashboard;

[Authorize(Policy = "PatientOnly")]
public class IndexModel : PageModel
{
    private readonly HospitalDbContext _db;
    private readonly AppointmentService _appts;
    private readonly BillingService _billing;

    public IndexModel(HospitalDbContext db, AppointmentService appts, BillingService billing)
    { _db = db; _appts = appts; _billing = billing; }

    public Patient? CurrentPatient { get; set; }
    public List<Appointment> UpcomingAppointments { get; set; } = new();
    public List<Appointment> PastAppointments { get; set; } = new();
    public List<MedicalRecord> RecentRecords { get; set; } = new();
    public List<Bill> RecentBills { get; set; } = new();
    public List<Doctor> AvailableDoctors { get; set; } = new();
    public int TotalAppointments { get; set; }
    public decimal OutstandingBalance { get; set; }

    public void OnGet()
    {
        var patientId = User.GetPatientId();
        if (patientId == null) return;

        CurrentPatient = _db.Patients.Find(patientId.Value);
        if (CurrentPatient == null) return;

        var allAppts = _appts.GetByPatient(patientId.Value);
        UpcomingAppointments = allAppts
            .Where(a => a.AppointmentDate >= DateTime.Now
                     && a.Status != AppointmentStatus.Cancelled
                     && a.Status != AppointmentStatus.Completed)
            .OrderBy(a => a.AppointmentDate).Take(5).ToList();

        PastAppointments = allAppts
            .Where(a => a.AppointmentDate < DateTime.Now
                     || a.Status == AppointmentStatus.Completed)
            .OrderByDescending(a => a.AppointmentDate).Take(3).ToList();

        TotalAppointments = allAppts.Count;

        RecentRecords = _db.MedicalRecords
            .Where(r => r.PatientId == patientId.Value)
            .OrderByDescending(r => r.RecordDate)
            .Take(3).ToList();

        var bills = _billing.GetByPatient(patientId.Value);
        RecentBills = bills.Take(3).ToList();
        OutstandingBalance = bills
            .Where(b => b.Status != BillStatus.Paid && b.Status != BillStatus.Cancelled)
            .Sum(b => b.Balance);

        AvailableDoctors = _db.Doctors
            .Where(d => d.IsAvailable)
            .OrderBy(d => d.Specialization)
            .ToList();
    }
}
