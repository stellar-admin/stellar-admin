using Microsoft.AspNetCore.Identity;

namespace DashboardPlayground.Data;

public class ApplicationUser : IdentityUser
{
    public Department? Department { get; set; }

    public Guid? DepartmentId { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string? PreferredLanguage { get; set; }

    public string? TimeZoneId { get; set; }
}
