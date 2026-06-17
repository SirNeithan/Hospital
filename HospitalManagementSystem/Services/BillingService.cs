using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services;

public class BillingService
{
    private readonly DataStore _store;

    public BillingService(DataStore store) => _store = store;

    public Bill GenerateBill(int patientId, List<BillItem> items,
        string insuranceProvider = "", string policyNumber = "", decimal insuranceCoverage = 0)
    {
        var patient = _store.Patients.FirstOrDefault(p => p.Id == patientId);

        var bill = new Bill
        {
            Id = _store.NextBillId(),
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
        _store.Bills.Add(bill);
        return bill;
    }

    public Bill? GetById(int id) =>
        _store.Bills.FirstOrDefault(b => b.Id == id);

    public List<Bill> GetByPatient(int patientId) =>
        _store.Bills.Where(b => b.PatientId == patientId).OrderByDescending(b => b.BillDate).ToList();

    public List<Bill> GetAll() => _store.Bills;

    public List<Bill> GetUnpaid() =>
        _store.Bills.Where(b => b.Status == BillStatus.Pending ||
                                b.Status == BillStatus.PartiallyPaid ||
                                b.Status == BillStatus.Overdue).ToList();

    public (bool Success, string Message) RecordPayment(int billId, decimal amount)
    {
        var bill = GetById(billId);
        if (bill == null) return (false, "Bill not found.");
        if (bill.Status == BillStatus.Paid) return (false, "Bill is already fully paid.");
        if (bill.Status == BillStatus.Cancelled) return (false, "Bill has been cancelled.");
        if (amount <= 0) return (false, "Payment amount must be greater than zero.");

        decimal remaining = bill.Balance;
        bill.AmountPaid += Math.Min(amount, remaining);
        bill.UpdateStatus();

        return (true, $"Payment of ${Math.Min(amount, remaining):F2} recorded. Remaining balance: ${bill.Balance:F2}");
    }

    public decimal GetTotalRevenue() =>
        _store.Bills.Sum(b => b.AmountPaid);

    public decimal GetOutstandingBalance() =>
        _store.Bills.Where(b => b.Status != BillStatus.Paid && b.Status != BillStatus.Cancelled)
                    .Sum(b => b.Balance);
}
