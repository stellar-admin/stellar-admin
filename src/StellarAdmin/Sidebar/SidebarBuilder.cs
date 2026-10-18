namespace StellarAdmin.Sidebar;

/// <summary>
///     Configures the sidebar.
/// </summary>
public sealed class SidebarBuilder
{
    private readonly SidebarOptions _options;

    /// <summary>
    ///     Sets a value that indicates whether the groups and the links outside any group are
    ///     sorted by label.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to sort them by label; otherwise, <see langword="false" /> to
    ///     keep the order they're added in. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Groups and links with an order come first, sorted by order.
    /// </remarks>
    public bool SortGroupsByLabel
    {
        set => _options.SortGroupsByLabel = value;
    }

    /// <summary>
    ///     Sets a value that indicates whether the items in each group are sorted by label.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to sort them by label; otherwise, <see langword="false" /> to
    ///     keep the order they're added in. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Items with an order come first, sorted by order. A group can override this setting.
    /// </remarks>
    public bool SortItemsByLabel
    {
        set => _options.SortItemsByLabel = value;
    }

    internal SidebarBuilder(SidebarOptions options) => _options = options;
}
