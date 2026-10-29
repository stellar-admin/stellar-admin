using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

public class Departure
{
    public int Id { get; set; }

    public int TripId { get; set; }

    public Trip Trip { get; set; } = null!;

    [Display(Name = "Start date")]
    public DateOnly StartDate { get; set; }

    [Display(Name = "End date")]
    public DateOnly EndDate { get; set; }

    [Display(Name = "Meeting time")]
    public TimeOnly? MeetingTime { get; set; }

    [Range(1, 40)]
    public int Capacity { get; set; }

    [Precision(10, 2)]
    [Display(Name = "Price override")]
    public decimal? PriceOverride { get; set; }

    [Display(Name = "Lead guide")]
    public int? LeadGuideId { get; set; }

    [Display(Name = "Lead guide")]
    public Guide? LeadGuide { get; set; }

    public DepartureStatus Status { get; set; } = DepartureStatus.Scheduled;

    public List<Booking> Bookings { get; set; } = [];

    [Display(Name = "Room allocations")]
    public List<RoomAllocation> RoomAllocations { get; set; } = [];
}
