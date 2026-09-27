using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Identity;

public sealed class EditUserModel
{
    [Display(Name = "Department")]
    public Guid? DepartmentId { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Display(Name = "Email confirmed")]
    public bool EmailConfirmed { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "First name")]
    public required string FirstName { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Last name")]
    public required string LastName { get; set; }

    [StringLength(35)]
    [Display(Name = "Preferred language")]
    public string? PreferredLanguage { get; set; }

    [StringLength(35)]
    [Display(Name = "Time zone")]
    public string? TimeZoneId { get; set; }
}
