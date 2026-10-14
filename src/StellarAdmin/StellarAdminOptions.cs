using StellarAdmin.Sidebar;

namespace StellarAdmin;

/// <summary>
///     Options for the StellarAdmin admin panel.
/// </summary>
public class StellarAdminOptions
{
    /// <summary>
    ///     Gets the app-relative paths of the scripts the shell layout links, in registration order.
    /// </summary>
    public IList<string> Scripts { get; } = [];

    /// <summary>
    ///     Gets the app-relative paths of the stylesheets the shell layout links, in registration order.
    /// </summary>
    public IList<string> Stylesheets { get; } = [];

    internal List<object> AuthorizationMetadata { get; } = [];

    internal List<SidebarLinkOptions> SidebarLinks { get; } = [];
}
