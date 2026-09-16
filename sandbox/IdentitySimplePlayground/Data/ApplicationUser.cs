using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentitySimplePlayground.Data;

[ModelMetadataType<ApplicationUserMeta>]
public class ApplicationUser : IdentityUser
{
    [UIHint("UserRoles")]
    public List<ApplicationRole> Roles { get; set; } = [];
}

public class ApplicationUserMeta
{
    [Display(Name = "Username")]
    public string UserName { get; set; }

    [UIHint("EmailConfirmed")]
    public bool EmailConfirmed { get; set; }

    [Display(Name = "Phone")]
    [Phone]
    public string? PhoneNumber { get; set; }
}
