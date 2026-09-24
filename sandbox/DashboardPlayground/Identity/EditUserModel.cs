using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Identity;

public sealed class EditUserModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Display(Name = "Email confirmed")]
    public bool EmailConfirmed { get; set; }
}
