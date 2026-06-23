using HospitalManagement.Data;
using HospitalManagement.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Rooms;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly HospitalDbContext _db;
    public IndexModel(HospitalDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public string Filter { get; set; } = "all";

    public List<Room> Rooms { get; set; } = new();

    public void OnGet()
    {
        var query = _db.Rooms.AsQueryable();
        Rooms = Filter == "available"
            ? query.Where(r => r.CurrentOccupancy < r.Capacity && r.Status != RoomStatus.UnderMaintenance).ToList()
            : query.OrderBy(r => r.Floor).ThenBy(r => r.RoomNumber).ToList();
    }

    public IActionResult OnPost(int id, string status)
    {
        var room = _db.Rooms.Find(id);
        if (room != null && Enum.TryParse<RoomStatus>(status, out var s))
        { room.Status = s; _db.SaveChanges(); }
        return RedirectToPage();
    }
}
