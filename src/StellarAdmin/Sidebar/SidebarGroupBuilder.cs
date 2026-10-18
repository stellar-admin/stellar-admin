namespace StellarAdmin.Sidebar;

/// <summary>
///     Configures a sidebar group.
/// </summary>
public sealed class SidebarGroupBuilder
{
    private readonly SidebarGroupOptions _options;

    /// <summary>
    ///     Sets the position of the group among the groups and the links outside any group.
    /// </summary>
    /// <value>
    ///     The position, where lower values come first, or <see langword="null" /> to come after
    ///     the groups and links with a position. The default is <see langword="null" />.
    /// </value>
    public int? Order
    {
        set => _options.Order = value;
    }

    /// <summary>
    ///     Sets a value that indicates whether the items in the group are sorted by label.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to sort them by label, <see langword="false" /> to keep the
    ///     order they're added in, or <see langword="null" /> to follow the sidebar's setting. The
    ///     default is <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     Items with an order come first, sorted by order.
    /// </remarks>
    public bool? SortItemsByLabel
    {
        set => _options.SortItemsByLabel = value;
    }

    internal SidebarGroupBuilder(SidebarGroupOptions options) => _options = options;
}
