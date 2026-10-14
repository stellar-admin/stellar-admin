using Microsoft.AspNetCore.Identity;

namespace AdminPlayground.Data;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
}
