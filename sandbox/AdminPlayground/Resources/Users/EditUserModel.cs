using System.ComponentModel.DataAnnotations;
using AdminPlayground.Data;

namespace AdminPlayground.Resources.Users;

// The user, with the validation of its fields
public sealed class EditUserModel : IValidatableObject
{
    public ApplicationUser User { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
        UserFormValidation.Validate(User);
}
