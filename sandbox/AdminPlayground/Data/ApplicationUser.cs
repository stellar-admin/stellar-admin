using Microsoft.AspNetCore.Identity;

namespace AdminPlayground.Data;

public class ApplicationUser : IdentityUser
{
    public Department? Department { get; set; }

    public Guid? DepartmentId { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string? PreferredLanguage { get; set; }

    // Identity's user roles, as a many-to-many navigation through its AspNetUserRoles join table
    public ICollection<ApplicationRole> Roles { get; } = new List<ApplicationRole>();

    public string? TimeZoneId { get; set; }
}
