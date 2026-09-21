using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Models;

public sealed class EditCustomerModel
{
    [Required]
    [Display(Name = "Customer name")]
    public string DisplayName { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";
}
