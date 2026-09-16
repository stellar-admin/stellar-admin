using System.ComponentModel.DataAnnotations;

namespace StellarAdmin.Dashboard.Identity.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The password inputs of the create user page. The single owner of the password
///     form contract: <see cref="UserCreateViewModel" /> composes it for rendering and
///     the POST action binds it under the same <c>PasswordInput</c> prefix, so client
///     and server validation cannot drift. Deliberately carries no length or complexity
///     annotations — the configured Identity password policy is the single source of
///     truth, applied by <c>CreateAsync</c>.
/// </summary>
public sealed class CreateUserPasswordInput
{
    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";
}
