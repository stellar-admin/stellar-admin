using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

public class RoomAllocation
{
    public int Id { get; set; }

    public int DepartureId { get; set; }

    public Departure Departure { get; set; } = null!;

    public int AccommodationId { get; set; }

    public Accommodation Accommodation { get; set; } = null!;

    [Range(1, 60)]
    public int Nights { get; set; }

    [Range(1, 40)]
    public int RoomsReserved { get; set; }

    [Precision(10, 2)]
    public decimal CostPerRoomNight { get; set; }

    public AllocationStatus Status { get; set; } = AllocationStatus.Requested;
}
