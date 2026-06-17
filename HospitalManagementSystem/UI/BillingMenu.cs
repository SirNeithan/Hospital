using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;

namespace HospitalManagementSystem.UI;

public class BillingMenu
{
    private readonly BillingService _billingService;
    private readonly PatientService _patientService;

    public BillingMenu(BillingService billingService, PatientService patientService)
    {
        _billingService = billingService;
        _patientService = patientService;
    }

    public void Show()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Billing & Payments");
            Console.WriteLine("  1. Generate Bill");
            Console.WriteLine("  2. View All Bills");
            Console.WriteLine("  3. View Patient Bills");
            Console.WriteLine("  4. View Bill Details");
            Console.WriteLine("  5. Record Payment");
            Console.WriteLine("  6. View Unpaid Bills");
            Console.WriteLine("  7. Financial Summary");
            Console.WriteLine("  0. Back to Main Menu");
            Console.WriteLine();

            string choice = ConsoleHelper.ReadInput("Select option");
            switch (choice)
            {
                case "1": GenerateBill(); break;
                case "2": ViewAll(); break;
                case "3": ViewByPatient(); break;
                case "4": ViewDetails(); break;
                case "5": RecordPayment(); break;
                case "6": ViewUnpaid(); break;
                case "7": FinancialSummary(); break;
                case "0": return;
                default: ConsoleHelper.PrintError("Invalid option."); break;
            }
        }
    }

    private void GenerateBill()
    {
        ConsoleHelper.PrintSubHeader("Generate Bill");

        Console.WriteLine("\n  Patients:");
        foreach (var p in _patientService.GetAll())
            Console.WriteLine($"    {p}");
        int patientId = ConsoleHelper.ReadInt("Patient ID");
        var patient = _patientService.GetById(patientId);
        if (patient == null) { ConsoleHelper.PrintError("Patient not found."); ConsoleHelper.PressAnyKey(); return; }

        var items = new List<BillItem>();
        Console.WriteLine("\n  Add billing items:");

        while (true)
        {
            Console.WriteLine("\n  Item types:");
            Console.WriteLine("  1. Consultation Fee");
            Console.WriteLine("  2. Room Charge");
            Console.WriteLine("  3. Lab Test");
            Console.WriteLine("  4. Medication");
            Console.WriteLine("  5. Surgery/Procedure");
            Console.WriteLine("  6. Custom Item");
            Console.WriteLine("  0. Done adding items");
            Console.Write("  Choice: ");
            string itemChoice = Console.ReadLine()?.Trim() ?? "0";

            if (itemChoice == "0") break;

            string description = itemChoice switch
            {
                "1" => "Consultation Fee",
                "2" => "Room Charge",
                "3" => ConsoleHelper.ReadInput("Lab test name"),
                "4" => ConsoleHelper.ReadInput("Medication name"),
                "5" => ConsoleHelper.ReadInput("Procedure name"),
                "6" => ConsoleHelper.ReadInput("Item description"),
                _ => ""
            };

            if (string.IsNullOrEmpty(description)) { ConsoleHelper.PrintError("Invalid choice."); continue; }

            int qty = ConsoleHelper.ReadInt("Quantity", 1, 1000);
            decimal price = ConsoleHelper.ReadDecimal("Unit Price ($)");

            items.Add(new BillItem { Description = description, Quantity = qty, UnitPrice = price });
            ConsoleHelper.PrintSuccess($"Added: {description} x{qty} @ ${price:F2}");
        }

        if (items.Count == 0) { ConsoleHelper.PrintWarning("No items added. Bill not created."); ConsoleHelper.PressAnyKey(); return; }

        string insuranceProvider = ConsoleHelper.ReadOptionalInput("Insurance Provider (optional)");
        string policyNumber = string.Empty;
        decimal coverage = 0;

        if (!string.IsNullOrEmpty(insuranceProvider))
        {
            policyNumber = ConsoleHelper.ReadOptionalInput("Policy Number");
            coverage = ConsoleHelper.ReadDecimal("Insurance Coverage Amount ($)");
        }

        var bill = _billingService.GenerateBill(patientId, items, insuranceProvider, policyNumber, coverage);
        ConsoleHelper.PrintSuccess($"Bill #{bill.Id} generated successfully.");
        PrintBillDetails(bill);
        ConsoleHelper.PressAnyKey();
    }

    private void ViewAll()
    {
        ConsoleHelper.PrintSubHeader("All Bills");
        var bills = _billingService.GetAll().OrderByDescending(b => b.BillDate).ToList();
        if (bills.Count == 0)
            ConsoleHelper.PrintInfo("No bills generated yet.");
        else
        {
            foreach (var b in bills)
                PrintBillSummary(b);
            ConsoleHelper.PrintInfo($"Total: {bills.Count} bills");
        }
        ConsoleHelper.PressAnyKey();
    }

    private void ViewByPatient()
    {
        ConsoleHelper.PrintSubHeader("Patient Bills");
        int patientId = ConsoleHelper.ReadInt("Patient ID");
        var patient = _patientService.GetById(patientId);
        if (patient == null) { ConsoleHelper.PrintError("Patient not found."); ConsoleHelper.PressAnyKey(); return; }

        ConsoleHelper.PrintInfo($"Bills for {patient.FullName}:");
        var bills = _billingService.GetByPatient(patientId);
        if (bills.Count == 0)
            ConsoleHelper.PrintInfo("No bills found.");
        else
            foreach (var b in bills)
                PrintBillSummary(b);
        ConsoleHelper.PressAnyKey();
    }

    private void ViewDetails()
    {
        ConsoleHelper.PrintSubHeader("Bill Details");
        int id = ConsoleHelper.ReadInt("Bill ID");
        var bill = _billingService.GetById(id);
        if (bill == null) { ConsoleHelper.PrintError("Bill not found."); ConsoleHelper.PressAnyKey(); return; }
        PrintBillDetails(bill);
        ConsoleHelper.PressAnyKey();
    }

    private void RecordPayment()
    {
        ConsoleHelper.PrintSubHeader("Record Payment");
        int billId = ConsoleHelper.ReadInt("Bill ID");
        var bill = _billingService.GetById(billId);
        if (bill == null) { ConsoleHelper.PrintError("Bill not found."); ConsoleHelper.PressAnyKey(); return; }

        Console.WriteLine($"  Patient:          {bill.PatientName}");
        Console.WriteLine($"  Total Amount:     ${bill.TotalAmount:F2}");
        Console.WriteLine($"  Amount Paid:      ${bill.AmountPaid:F2}");
        Console.ForegroundColor = bill.Balance > 0 ? ConsoleColor.Yellow : ConsoleColor.Green;
        Console.WriteLine($"  Outstanding:      ${bill.Balance:F2}");
        Console.ResetColor();

        decimal amount = ConsoleHelper.ReadDecimal("Payment Amount ($)");
        var (success, message) = _billingService.RecordPayment(billId, amount);
        if (success)
            ConsoleHelper.PrintSuccess(message);
        else
            ConsoleHelper.PrintError(message);
        ConsoleHelper.PressAnyKey();
    }

    private void ViewUnpaid()
    {
        ConsoleHelper.PrintSubHeader("Unpaid Bills");
        var bills = _billingService.GetUnpaid();
        if (bills.Count == 0)
            ConsoleHelper.PrintSuccess("No unpaid bills.");
        else
        {
            foreach (var b in bills)
                PrintBillSummary(b);
            ConsoleHelper.PrintWarning($"Total outstanding: ${bills.Sum(b => b.Balance):F2}");
        }
        ConsoleHelper.PressAnyKey();
    }

    private void FinancialSummary()
    {
        ConsoleHelper.PrintSubHeader("Financial Summary");
        var bills = _billingService.GetAll();
        Console.WriteLine();
        Console.WriteLine($"  Total Bills Generated:   {bills.Count}");
        Console.WriteLine($"  Total Revenue (Billed):  ${bills.Sum(b => b.TotalAmount):F2}");
        Console.WriteLine($"  Total Collected:         ${_billingService.GetTotalRevenue():F2}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  Outstanding Balance:     ${_billingService.GetOutstandingBalance():F2}");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine($"  Bills by Status:");
        foreach (var status in Enum.GetValues<BillStatus>())
        {
            int count = bills.Count(b => b.Status == status);
            if (count > 0)
                Console.WriteLine($"    {status,-20} {count,5}");
        }
        ConsoleHelper.PressAnyKey();
    }

    private void PrintBillSummary(Bill b)
    {
        Console.ForegroundColor = b.Status switch
        {
            BillStatus.Paid => ConsoleColor.Green,
            BillStatus.Overdue => ConsoleColor.Red,
            BillStatus.PartiallyPaid => ConsoleColor.Yellow,
            BillStatus.Cancelled => ConsoleColor.DarkGray,
            _ => ConsoleColor.White
        };
        Console.WriteLine($"  {b}");
        Console.ResetColor();
    }

    private void PrintBillDetails(Bill bill)
    {
        ConsoleHelper.PrintSeparator();
        Console.WriteLine($"  BILL #{bill.Id}");
        Console.WriteLine($"  Patient:   {bill.PatientName}");
        Console.WriteLine($"  Date:      {bill.BillDate:dd/MM/yyyy}");
        Console.WriteLine($"  Due Date:  {bill.DueDate:dd/MM/yyyy}");
        Console.WriteLine();
        Console.WriteLine($"  {"DESCRIPTION",-35} {"QTY",4}  {"UNIT PRICE",10}  {"TOTAL",10}");
        Console.WriteLine(new string('-', 65));
        foreach (var item in bill.Items)
            Console.WriteLine(item);
        Console.WriteLine(new string('-', 65));
        Console.WriteLine($"  {"Subtotal",-50} ${bill.SubTotal,9:F2}");
        Console.WriteLine($"  {"Tax (8%)",-50} ${bill.Tax,9:F2}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  {"TOTAL",-50} ${bill.TotalAmount,9:F2}");
        Console.ResetColor();
        if (bill.InsuranceCoverage > 0)
            Console.WriteLine($"  {"Insurance Coverage (" + bill.InsuranceProvider + ")",-50} -${bill.InsuranceCoverage,8:F2}");
        Console.WriteLine($"  {"Amount Paid",-50} ${bill.AmountPaid,9:F2}");
        Console.ForegroundColor = bill.Balance > 0 ? ConsoleColor.Yellow : ConsoleColor.Green;
        Console.WriteLine($"  {"BALANCE DUE",-50} ${bill.Balance,9:F2}");
        Console.ResetColor();
        Console.WriteLine($"  Status: {bill.Status}");
        ConsoleHelper.PrintSeparator();
    }
}
