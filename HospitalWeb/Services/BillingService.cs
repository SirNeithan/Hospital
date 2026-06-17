using HospitalWeb.Data;
using HospitalWeb.Models;

namespace HospitalWeb.Services;

public class BillingService
{
    private readonly HospitalDbContext _db;

    public BillingService(HospitalDbContext db) => _db = db;

    public Bill GenerateBill(int patientId, List<BillItem> items,
        string insuranceProvider = "", string policyNumber = "", decimal insuranceCoverage = 0)
    {
        var patient = _db.Patients.Find(patientId);

        foreach (var item in items)
            item.UnitPrice = Math.Clamp(item.UnitPrice, 50000, 100000);

        var bill = new Bill
        {
            PatientId = patientId,
            PatientName = patient?.FullName ?? "Unknown",
            BillDate = DateTime.Now,
            DueDate = DateTime.Now.AddDays(30),
            Items = items,
            InsuranceProvider = insuranceProvider,
            InsurancePolicyNumber = policyNumber,
            InsuranceCoverage = insuranceCoverage,
            AmountPaid = insuranceCoverage
        };
        bill.UpdateStatus();
        _db.Bills.Add(bill);
        _db.SaveChanges();
        return bill;
    }

    public Bill? GetById(int id) => _db.Bills.Find(id);

    public List<Bill> GetByPatient(int patientId) =>
        _db.Bills.Where(b => b.PatientId == patientId)
            .OrderByDescending(b => b.BillDate).ToList();

    public List<Bill> GetAll() =>
        _db.Bills.OrderByDescending(b => b.BillDate).ToList();

    public List<Bill> GetUnpaid() =>
        _db.Bills.Where(b => b.Status == BillStatus.Pending ||
                             b.Status == BillStatus.PartiallyPaid ||
                             b.Status == BillStatus.Overdue).ToList();

    public (bool Success, string Message) RecordPayment(int billId, decimal amount)
    {
        var bill = GetById(billId);
        if (bill == null) return (false, "Bill not found.");
        if (bill.Status == BillStatus.Paid) return (false, "Bill is already fully paid.");
        if (bill.Status == BillStatus.Cancelled) return (false, "Bill has been cancelled.");
        if (amount <= 0) return (false, "Payment amount must be greater than zero.");

        decimal paying = Math.Min(amount, bill.Balance);
        bill.AmountPaid += paying;
        bill.UpdateStatus();
        _db.SaveChanges();
        return (true, $"Payment of UGX {paying:N0} recorded. Remaining balance: UGX {bill.Balance:N0}");
    }

    public decimal GetTotalRevenue() => _db.Bills.Sum(b => (decimal?)b.AmountPaid) ?? 0;

    public decimal GetOutstandingBalance() =>
        _db.Bills
           .Where(b => b.Status != BillStatus.Paid && b.Status != BillStatus.Cancelled)
           .AsEnumerable()
           .Sum(b => b.Balance);
}
