using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VoyagerTravel.Data;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;
}
