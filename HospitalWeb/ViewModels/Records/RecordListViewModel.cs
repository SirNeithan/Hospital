using HospitalManagement.Core;
using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Records;

public class RecordListViewModel
{
    public PagedResult<MedicalRecord> Records { get; set; } = null!;
    public string? Search    { get; set; }
    public int     PageNumber { get; set; } = 1;
}
