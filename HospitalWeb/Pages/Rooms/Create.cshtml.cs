using HospitalWeb.Data;
using HospitalWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HospitalWeb.Pages.Rooms;

public class CreateModel : PageModel
{
    private readonly HospitalDbContext _db;
    public CreateModel(HospitalDbContext db) => _db = db;

    [BindProperty][Required] public string RoomNumber { get; set; } = "";
    [BindProperty] public RoomType Type { get; set; }
    [BindProperty] public int Capacity { get; set; } = 1;
    [BindProperty] public decimal DailyRate { get; set; }
    [BindProperty][Required] public string Floor { get; set; } = "";
    [BindProperty][Required] public string Ward { get; set; } = "";

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();
        var room = new Room { RoomNumber = RoomNumber, Type = Type, Capacity = Capacity, DailyRate = DailyRate, Floor = Floor, Ward = Ward };
        _db.Rooms.Add(room);
        _db.SaveChanges();
        TempData["Success"] = $"Room {RoomNumber} added.";
        return RedirectToPage("./Index");
    }
}
