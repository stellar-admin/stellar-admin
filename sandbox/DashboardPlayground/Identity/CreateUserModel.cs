using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Identity;

public sealed class CreateUserModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Display(Name = "Email confirmed")]
    public bool EmailConfirmed { get; set; }

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password)), DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    public string PasswordConfirmation { get; set; } = "";
}
