using HospitalWeb.Models;
using HospitalWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Records;

public class DetailsModel : PageModel
{
    private readonly MedicalRecordService _records;
    public DetailsModel(MedicalRecordService records) => _records = records;

    public MedicalRecord? Record { get; set; }

    public IActionResult OnGet(int id)
    {
        Record = _records.GetById(id);
        if (Record == null) return NotFound();
        return Page();
    }
}
