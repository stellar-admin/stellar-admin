using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VoyagerTravel.Data;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    [Display(Name = "Display name")]
    public string DisplayName { get; set; } = string.Empty;
}
