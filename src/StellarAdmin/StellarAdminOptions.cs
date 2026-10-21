using StellarAdmin.Home;
using StellarAdmin.Sidebar;

namespace StellarAdmin;

internal sealed class StellarAdminOptions
{
    private int _sidebarPositionCount;

    internal IList<string> Scripts { get; } = [];

    internal IList<string> Stylesheets { get; } = [];

    internal List<object> AuthorizationMetadata { get; } = [];

    internal BrandingOptions Branding { get; } = new();

    internal EnvironmentIndicator? EnvironmentIndicator { get; set; }

    internal HomePageOptions HomePage { get; } = new();

    internal List<SidebarLinkOptions> SidebarLinks { get; } = [];

    internal SidebarOptions Sidebar { get; } = new();

    internal UserMenuOptions UserMenu { get; } = new();

    // Resources and links share one sequence, so items without an order keep the order of the
    // calls that added them
    internal int NextSidebarPosition() => _sidebarPositionCount++;
}
