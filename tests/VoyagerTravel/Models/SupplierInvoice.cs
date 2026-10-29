using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

[Index(nameof(SupplierId), nameof(InvoiceNumber), IsUnique = true)]
public class SupplierInvoice
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public Supplier Supplier { get; set; } = null!;

    public int? DepartureId { get; set; }

    public Departure? Departure { get; set; }

    [MaxLength(30)]
    [Display(Name = "Invoice number")]
    public required string InvoiceNumber { get; set; }

    [Display(Name = "Issued on")]
    public DateOnly IssuedOn { get; set; }

    [Display(Name = "Due on")]
    public DateOnly DueOn { get; set; }

    [Precision(10, 2)]
    public decimal Amount { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Received;
}
