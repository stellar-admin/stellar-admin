namespace DashboardPlayground.Data;

public class Department
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public ICollection<ApplicationUser> Users { get; } = new List<ApplicationUser>();
}
