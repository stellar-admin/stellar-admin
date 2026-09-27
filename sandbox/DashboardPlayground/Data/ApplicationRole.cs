using Microsoft.AspNetCore.Identity;

namespace DashboardPlayground.Data;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
}
