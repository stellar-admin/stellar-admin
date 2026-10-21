using StellarAdmin.Diagnostics;
using StellarAdmin.Sidebar;

namespace StellarAdmin.Resources;

internal sealed record ResourceRegistration(
    Type ResourceType,
    Type ControllerType,
    string ControllerName,
    int SidebarPosition,
    Func<IServiceProvider, ResourceSidebarItem> ResolveSidebarItem,
    Func<IServiceProvider, IReadOnlyList<object>> ResolveAuthorizationMetadata,
    Func<IServiceProvider, bool> ResolveCanCreate,
    Func<IServiceProvider, IReadOnlyList<ConfigurationProblem>> ResolveProblems
);
