using System.ComponentModel.DataAnnotations;
using AdminPlayground.Data;

namespace AdminPlayground.Resources.Users;

// The user, and the roles that UserManager saves for it
public sealed class EditUserModel : IValidatableObject
{
    public ApplicationUser User { get; set; } = null!;

    [Display(Name = "Roles")]
    public string[] RoleIds { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
        UserFormValidation.Validate(User);
}
