namespace StellarAdmin.Pro.Identity.Options;

/// <summary>
///     The configured options for StellarAdmin Identity management.
/// </summary>
public class StellarAdminIdentityOptions
{
    /// <summary>
    ///     Sidebar configuration options related to the sidebar group and items.
    /// </summary>
    public IdentitySidebarOptions Sidebar { get; } = new();
}
