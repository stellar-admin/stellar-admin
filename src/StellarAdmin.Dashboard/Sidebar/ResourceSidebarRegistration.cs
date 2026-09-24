namespace StellarAdmin.Dashboard.Sidebar;

internal sealed record ResourceSidebarRegistration(
    Type ResourceType,
    string ControllerName,
    Func<IServiceProvider, ResourceSidebarItem> Resolve
);

internal sealed record ResourceSidebarItem(string Label, string Group, int Order, bool Visible);
