using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.Roles;

public sealed class RoleFormModel
{
    [Required]
    public string Name { get; set; } = "";
}
