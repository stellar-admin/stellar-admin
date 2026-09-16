namespace StellarAdmin.Pro.Identity.Options;

/// <summary>
///     The configured options for the Identity section of the StellarAdmin sidebar.
///     Read-only outside the configuration builders.
/// </summary>
public class IdentitySidebarOptions
{
    /// <summary>
    ///     The options for the roles sidebar item.
    /// </summary>
    public SidebarItemOptions RolesItem { get; } = new();

    /// <summary>
    ///     The sidebar section title, or <c>null</c> to use the default ("Identity").
    /// </summary>
    public string? Title { get; internal set; }

    /// <summary>
    ///     The options for the users sidebar item.
    /// </summary>
    public SidebarItemOptions UsersItem { get; } = new();

    /// <summary>
    ///     Whether the sidebar section renders. Cosmetic only — hiding the section does
    ///     not disable the user management screens.
    /// </summary>
    public bool Visible { get; internal set; } = true;
}
