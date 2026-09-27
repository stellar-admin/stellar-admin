using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.Roles;

public sealed class RoleFormModel
{
    [StringLength(200)]
    public string? Description { get; set; }

    [Required]
    public string Name { get; set; } = "";
}
