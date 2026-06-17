namespace HospitalWeb.Models;

public enum BillStatus
{
    Pending,
    PartiallyPaid,
    Paid,
    Overdue,
    Cancelled
}

public class BillItem
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Total => Quantity * UnitPrice;

    public override string ToString() =>
        $"  {Description,-35} x{Quantity,3}   ${UnitPrice,8:F2}   ${Total,9:F2}";
}

public class Bill
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime BillDate { get; set; } = DateTime.Now;
    public DateTime DueDate { get; set; }
    public List<BillItem> Items { get; set; } = new();
    public decimal SubTotal => Items.Sum(i => i.Total);
    public decimal TaxRate { get; set; } = 0.08m; // 8% tax
    public decimal Tax => SubTotal * TaxRate;
    public decimal TotalAmount => SubTotal + Tax;
    public decimal AmountPaid { get; set; } = 0;
    public decimal Balance => TotalAmount - AmountPaid;
    public BillStatus Status { get; set; } = BillStatus.Pending;
    public string InsuranceProvider { get; set; } = string.Empty;
    public string InsurancePolicyNumber { get; set; } = string.Empty;
    public decimal InsuranceCoverage { get; set; } = 0;
    public string Notes { get; set; } = string.Empty;
    public void UpdateStatus()
    {
        if (AmountPaid <= 0)
            Status = DateTime.Now > DueDate ? BillStatus.Overdue : BillStatus.Pending;
        else if (AmountPaid >= TotalAmount)
            Status = BillStatus.Paid;
        else
            Status = BillStatus.PartiallyPaid;
    }

    public override string ToString() =>
        $"[{Id}] {BillDate:dd/MM/yyyy} | Patient: {PatientName} | Total: ${TotalAmount:F2} | Paid: ${AmountPaid:F2} | Balance: ${Balance:F2} | {Status}";
}
