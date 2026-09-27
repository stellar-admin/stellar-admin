using StellarAdmin.Dashboard.Sidebar;

namespace StellarAdmin.Dashboard.Resources;

internal sealed record ResourceRegistration(
    Type ControllerType,
    string ControllerName,
    Func<IServiceProvider, ResourceSidebarItem> ResolveSidebarItem,
    Func<IServiceProvider, IReadOnlyList<object>> ResolveAuthorizationMetadata
);
