using System.ComponentModel.DataAnnotations;
using AdminPlayground.Data;

namespace AdminPlayground.Resources.Users;

// The user, and the password that UserManager saves with it
public sealed class CreateUserModel : IValidatableObject
{
    public ApplicationUser User { get; set; } = new() { FirstName = "", LastName = "" };

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password)), DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    public string PasswordConfirmation { get; set; } = "";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
        UserFormValidation.Validate(User);
}
