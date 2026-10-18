using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a resource's sidebar item.
/// </summary>
/// <typeparam name="TResource">The type of the resource the item links to.</typeparam>
public sealed class ResourceSidebarItemBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets the label of the sidebar group the item appears in.
    /// </summary>
    /// <value>
    ///     The group label. The default is "Resources".
    /// </value>
    /// <remarks>
    ///     The label can't be empty or whitespace.
    /// </remarks>
    public string Group
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.SidebarItem.Group = value
            );
    }

    /// <summary>
    ///     Sets the icon shown before the item's label.
    /// </summary>
    /// <value>
    ///     The name of the icon, or <see langword="null" /> for none. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     The name can't be empty or whitespace.
    /// </remarks>
    public string? Icon
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.SidebarItem.Icon = value
            );
    }

    /// <summary>
    ///     Sets the label of the item.
    /// </summary>
    /// <value>
    ///     The label, or <see langword="null" /> to use the resource's plural label. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     The label can't be empty or whitespace.
    /// </remarks>
    public string? Label
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.SidebarItem.Label = value
            );
    }

    /// <summary>
    ///     Sets the position of the item within its group.
    /// </summary>
    /// <value>
    ///     The position, where lower values come first, or <see langword="null" /> to come after
    ///     the items with a position, in the order they're added. The default is
    ///     <see langword="null" />.
    /// </value>
    public int? Order
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.SidebarItem.Order = value
            );
    }

    /// <summary>
    ///     Sets a value that indicates whether the item appears in the sidebar.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the item appears in the sidebar; otherwise,
    ///     <see langword="false" />. The default is <see langword="true" />.
    /// </value>
    /// <remarks>
    ///     Hiding the item doesn't disable the resource's pages.
    /// </remarks>
    public bool Visible
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.SidebarItem.Visible = value
            );
    }

    internal ResourceSidebarItemBuilder(IServiceCollection services) => _services = services;
}
