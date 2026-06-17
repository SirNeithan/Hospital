using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;

namespace HospitalManagementSystem.UI;

public class RoomMenu
{
    private readonly DataStore _store;

    public RoomMenu(DataStore store) => _store = store;

    public void Show()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Room & Ward Management");
            Console.WriteLine("  1. View All Rooms");
            Console.WriteLine("  2. View Available Rooms");
            Console.WriteLine("  3. View Rooms by Type");
            Console.WriteLine("  4. Add New Room");
            Console.WriteLine("  5. Update Room Status");
            Console.WriteLine("  0. Back to Main Menu");
            Console.WriteLine();

            string choice = ConsoleHelper.ReadInput("Select option");
            switch (choice)
            {
                case "1": ViewAll(); break;
                case "2": ViewAvailable(); break;
                case "3": ViewByType(); break;
                case "4": AddRoom(); break;
                case "5": UpdateStatus(); break;
                case "0": return;
                default: ConsoleHelper.PrintError("Invalid option."); break;
            }
        }
    }

    private void ViewAll()
    {
        ConsoleHelper.PrintSubHeader("All Rooms");
        if (_store.Rooms.Count == 0)
        {
            ConsoleHelper.PrintInfo("No rooms registered.");
        }
        else
        {
            foreach (var r in _store.Rooms.OrderBy(r => r.Floor).ThenBy(r => r.RoomNumber))
                PrintRoom(r);
            ConsoleHelper.PrintInfo($"Total: {_store.Rooms.Count} rooms");
        }
        ConsoleHelper.PressAnyKey();
    }

    private void ViewAvailable()
    {
        ConsoleHelper.PrintSubHeader("Available Rooms");
        var available = _store.Rooms.Where(r => r.HasAvailableBed && r.Status != RoomStatus.UnderMaintenance).ToList();
        if (available.Count == 0)
            ConsoleHelper.PrintWarning("No available rooms.");
        else
        {
            foreach (var r in available)
                PrintRoom(r);
            ConsoleHelper.PrintInfo($"Available: {available.Count} rooms");
        }
        ConsoleHelper.PressAnyKey();
    }

    private void ViewByType()
    {
        ConsoleHelper.PrintSubHeader("Filter Rooms by Type");
        RoomType type = ConsoleHelper.ReadEnum<RoomType>("Room Type");
        var rooms = _store.Rooms.Where(r => r.Type == type).ToList();
        if (rooms.Count == 0)
            ConsoleHelper.PrintInfo($"No rooms of type {type}.");
        else
            foreach (var r in rooms)
                PrintRoom(r);
        ConsoleHelper.PressAnyKey();
    }

    private void AddRoom()
    {
        ConsoleHelper.PrintSubHeader("Add New Room");
        string roomNumber = ConsoleHelper.ReadInput("Room Number");
        RoomType type = ConsoleHelper.ReadEnum<RoomType>("Room Type");
        int capacity = ConsoleHelper.ReadInt("Capacity (beds)", 1, 20);
        decimal rate = ConsoleHelper.ReadDecimal("Daily Rate ($)");
        string floor = ConsoleHelper.ReadInput("Floor");
        string ward = ConsoleHelper.ReadInput("Ward Name");

        var room = new Room
        {
            Id = _store.NextRoomId(),
            RoomNumber = roomNumber,
            Type = type,
            Capacity = capacity,
            DailyRate = rate,
            Floor = floor,
            Ward = ward
        };

        _store.Rooms.Add(room);
        ConsoleHelper.PrintSuccess($"Room {roomNumber} added successfully! ID: {room.Id}");
        ConsoleHelper.PressAnyKey();
    }

    private void UpdateStatus()
    {
        ConsoleHelper.PrintSubHeader("Update Room Status");
        ViewAll();
        int id = ConsoleHelper.ReadInt("Room ID");
        var room = _store.Rooms.FirstOrDefault(r => r.Id == id);
        if (room == null) { ConsoleHelper.PrintError("Room not found."); ConsoleHelper.PressAnyKey(); return; }

        Console.WriteLine($"  Current status: {room.Status}");
        RoomStatus newStatus = ConsoleHelper.ReadEnum<RoomStatus>("New Status");
        room.Status = newStatus;
        ConsoleHelper.PrintSuccess($"Room {room.RoomNumber} status updated to {newStatus}.");
        ConsoleHelper.PressAnyKey();
    }

    private void PrintRoom(Room r)
    {
        Console.ForegroundColor = r.Status switch
        {
            RoomStatus.Available => ConsoleColor.Green,
            RoomStatus.Occupied => ConsoleColor.Yellow,
            RoomStatus.UnderMaintenance => ConsoleColor.DarkRed,
            RoomStatus.Reserved => ConsoleColor.Cyan,
            _ => ConsoleColor.White
        };
        Console.WriteLine($"  {r}");
        Console.ResetColor();
    }
}
