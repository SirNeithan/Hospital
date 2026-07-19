using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Rooms;

public class RoomListViewModel
{
    public List<Room> Rooms  { get; set; } = new();
    public string     Filter { get; set; } = "all";
}
