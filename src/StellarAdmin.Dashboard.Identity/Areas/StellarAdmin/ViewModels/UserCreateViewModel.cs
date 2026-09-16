using StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels;

namespace StellarAdmin.Dashboard.Identity.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The view model of the create user page: the shared form plus the password inputs.
/// </summary>
public sealed class UserCreateViewModel : ResourceFormPageViewModel
{
    /// <summary>The password inputs rendered below the form fields.</summary>
    public CreateUserPasswordInput PasswordInput { get; set; } = new();
}
