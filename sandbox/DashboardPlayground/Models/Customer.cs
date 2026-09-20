using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Models;

public sealed class Customer
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = "";
}
