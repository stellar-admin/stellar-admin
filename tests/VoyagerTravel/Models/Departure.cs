using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

public class Departure
{
    public int Id { get; set; }

    public int TripId { get; set; }

    public Trip Trip { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public TimeOnly? MeetingTime { get; set; }

    [Range(1, 40)]
    public int Capacity { get; set; }

    [Precision(10, 2)]
    public decimal? PriceOverride { get; set; }

    public int? LeadGuideId { get; set; }

    public Guide? LeadGuide { get; set; }

    public DepartureStatus Status { get; set; } = DepartureStatus.Scheduled;

    public List<Booking> Bookings { get; set; } = [];

    public List<RoomAllocation> RoomAllocations { get; set; } = [];
}
