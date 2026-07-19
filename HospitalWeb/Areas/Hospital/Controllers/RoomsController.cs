using HospitalManagement.Core.Models;
using HospitalManagement.Data;
using HospitalWeb.ViewModels.Rooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HospitalWeb.Areas.Hospital.Controllers;

[Area("Hospital")]
[Authorize(Policy = "AdminOnly")]
public class RoomsController : Controller
{
    private readonly HospitalDbContext _db;
    public RoomsController(HospitalDbContext db) => _db = db;

    // GET /Rooms
    public IActionResult Index(string filter = "all")
    {
        var query = _db.Rooms.AsQueryable();
        var rooms = filter == "available"
            ? query.Where(r => r.CurrentOccupancy < r.Capacity &&
                               r.Status != RoomStatus.UnderMaintenance).ToList()
            : query.OrderBy(r => r.Floor).ThenBy(r => r.RoomNumber).ToList();

        var vm = new RoomListViewModel { Rooms = rooms, Filter = filter };
        return View(vm);
    }

    // GET /Rooms/Create
    public IActionResult Create() => View();

    // POST /Rooms/Create
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult Create(string roomNumber, RoomType type, int capacity,
        decimal dailyRate, string floor, string ward)
    {
        if (string.IsNullOrWhiteSpace(roomNumber))
        {
            ModelState.AddModelError("", "Room number is required.");
            return View();
        }
        var room = new Room
        {
            RoomNumber = roomNumber, Type = type, Capacity = capacity,
            DailyRate = dailyRate, Floor = floor, Ward = ward
        };
        _db.Rooms.Add(room);
        _db.SaveChanges();
        TempData["Success"] = $"Room {roomNumber} added.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Rooms/SetStatus
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult SetStatus(int id, string status)
    {
        var room = _db.Rooms.Find(id);
        if (room != null && Enum.TryParse<RoomStatus>(status, out var s))
        { room.Status = s; _db.SaveChanges(); }
        return RedirectToAction(nameof(Index));
    }
}
