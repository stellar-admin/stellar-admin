using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

public class Supplier
{
    public int Id { get; set; }

    [MaxLength(150)]
    public required string Name { get; set; }

    public SupplierType Type { get; set; }

    public int CountryId { get; set; }

    public Country Country { get; set; } = null!;

    [MaxLength(100)]
    [Display(Name = "Contact name")]
    public string? ContactName { get; set; }

    [MaxLength(254)]
    [EmailAddress]
    public required string Email { get; set; }

    [MaxLength(30)]
    [Phone]
    public string? Phone { get; set; }

    [MaxLength(500)]
    [Url]
    public string? Website { get; set; }

    [MaxLength(500)]
    [Display(Name = "Logo URL")]
    public string? LogoUrl { get; set; }

    public required Address Address { get; set; }

    [Display(Name = "Contract status")]
    public ContractStatus ContractStatus { get; set; } = ContractStatus.Prospect;

    [Precision(5, 2)]
    [Range(0, 100)]
    [Display(Name = "Commission rate")]
    public decimal CommissionRate { get; set; } = 10.00m;

    public List<Accommodation> Accommodations { get; set; } = [];

    public List<Excursion> Excursions { get; set; } = [];

    public List<SupplierInvoice> Invoices { get; set; } = [];
}
