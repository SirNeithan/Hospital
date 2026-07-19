using HospitalManagement.Core;
using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Billing;

public class BillingListViewModel
{
    public PagedResult<Bill> Bills    { get; set; } = null!;
    public string Filter              { get; set; } = "all";
    public int    PageNumber          { get; set; } = 1;
    public decimal TotalRevenue       { get; set; }
    public decimal Outstanding        { get; set; }
}
