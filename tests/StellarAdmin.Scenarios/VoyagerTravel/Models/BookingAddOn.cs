using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

public class BookingAddOn
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public Booking Booking { get; set; } = null!;

    public int ExcursionId { get; set; }

    public Excursion Excursion { get; set; } = null!;

    [Range(1, 20)]
    public int Quantity { get; set; } = 1;

    [Precision(10, 2)]
    public decimal UnitPrice { get; set; }
}
