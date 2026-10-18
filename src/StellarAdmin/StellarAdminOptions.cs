using StellarAdmin.Home;
using StellarAdmin.Sidebar;

namespace StellarAdmin;

internal sealed class StellarAdminOptions
{
    internal IList<string> Scripts { get; } = [];

    internal IList<string> Stylesheets { get; } = [];

    internal List<object> AuthorizationMetadata { get; } = [];

    internal BrandingOptions Branding { get; } = new();

    internal HomePageOptions HomePage { get; } = new();

    internal List<SidebarLinkOptions> SidebarLinks { get; } = [];

    internal UserMenuOptions UserMenu { get; } = new();
}
