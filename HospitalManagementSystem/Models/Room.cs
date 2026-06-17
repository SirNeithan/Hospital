namespace HospitalManagementSystem.Models;

public enum RoomType
{
    General,
    Private,
    ICU,
    Emergency,
    OperatingTheater,
    MaternityWard,
    PediatricWard
}

public enum RoomStatus
{
    Available,
    Occupied,
    UnderMaintenance,
    Reserved
}

public class Room
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public RoomType Type { get; set; }
    public RoomStatus Status { get; set; } = RoomStatus.Available;
    public int Capacity { get; set; } = 1;
    public int CurrentOccupancy { get; set; } = 0;
    public decimal DailyRate { get; set; }
    public string Floor { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;

    public bool HasAvailableBed => CurrentOccupancy < Capacity;

    public override string ToString() =>
        $"[{Id}] Room {RoomNumber} | {Type} | {Status} | Floor: {Floor} | Occupancy: {CurrentOccupancy}/{Capacity} | Rate: ${DailyRate:F2}/day";
}
