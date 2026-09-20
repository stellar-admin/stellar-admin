using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Models;

public sealed class CreateCustomerModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Name { get; set; } = "";

    [Required, MinLength(8), DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password)), DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    public string PasswordConfirmation { get; set; } = "";
}
