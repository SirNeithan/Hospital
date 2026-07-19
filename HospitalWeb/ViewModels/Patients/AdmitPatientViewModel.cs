using System.ComponentModel.DataAnnotations;
using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Patients;

public class AdmitPatientViewModel
{
    public Patient Patient { get; set; } = null!;
    public List<Room> AvailableRooms { get; set; } = new();

    [Range(1, int.MaxValue, ErrorMessage = "Please select a room.")]
    public int RoomId { get; set; }
}
