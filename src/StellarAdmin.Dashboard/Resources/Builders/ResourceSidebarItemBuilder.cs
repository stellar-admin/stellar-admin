using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a resource's sidebar item.
/// </summary>
public sealed class ResourceSidebarItemBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The sidebar group label.
    /// </summary>
    public string Group
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.SidebarItem.Group = value
            );
    }

    /// <summary>
    ///     The sidebar item label.
    /// </summary>
    public string? Label
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.SidebarItem.Label = value
            );
    }

    /// <summary>
    ///     The item's order within its group.
    /// </summary>
    public int Order
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.SidebarItem.Order = value
            );
    }

    /// <summary>
    ///     Whether the item appears in the sidebar.
    /// </summary>
    public bool Visible
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.SidebarItem.Visible = value
            );
    }

    internal ResourceSidebarItemBuilder(IServiceCollection services) => _services = services;
}
