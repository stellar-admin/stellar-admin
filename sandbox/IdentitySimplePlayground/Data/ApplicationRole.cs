using Microsoft.AspNetCore.Identity;

namespace IdentitySimplePlayground.Data;

public class ApplicationRole : IdentityRole
{
    public List<ApplicationUser> Users { get; set; } = [];
}