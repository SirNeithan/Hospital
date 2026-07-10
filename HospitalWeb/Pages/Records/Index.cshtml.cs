using Microsoft.AspNetCore.Mvc;
using HospitalManagement.Core;
using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Records;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly MedicalRecordService _records;
    public IndexModel(MedicalRecordService records) => _records = records;

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public const int PageSize = 15;

    public PagedResult<MedicalRecord> Records { get; set; } = null!;

    public void OnGet()
    {
        Records = PagedResult<MedicalRecord>.Create(
            _records.Query(Search),
            PageNumber, PageSize);
    }
}
