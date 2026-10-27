using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

public class Payment
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public Booking Booking { get; set; } = null!;

    [Precision(10, 2)]
    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; }

    public DateTime PaidAt { get; set; }

    [MaxLength(40)]
    public string? ProviderReference { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Succeeded;
}
