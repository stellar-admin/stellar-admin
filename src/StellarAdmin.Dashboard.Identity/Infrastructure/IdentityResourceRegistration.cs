using StellarAdmin.Dashboard.Identity.Options;

namespace StellarAdmin.Dashboard.Identity.Infrastructure;

// Describes one registered resource to the library code that treats every resource alike.
// This type is not generic, so a single injected collection covers them all regardless of
// the entity type each one is closed over.
internal sealed class IdentityResourceRegistration
{
    // The route value of the controller that serves the resource.
    public required string ControllerName { get; init; }

    // Resolved when the sidebar renders, so a title configured after registration still shows.
    public required Func<string> IndexTitle { get; init; }

    // The position of the resource within the sidebar group.
    public required int Order { get; init; }

    public required SidebarItemOptions SidebarItem { get; init; }
}
