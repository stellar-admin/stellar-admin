using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Identity;

public sealed class RoleFormModel
{
    [Required]
    public string Name { get; set; } = "";
}
