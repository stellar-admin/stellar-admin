using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.Customers;

public sealed class EditCustomerModel
{
    [Required]
    [Display(Name = "Customer name")]
    public string DisplayName { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";
}
