namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a resource's sidebar item.
/// </summary>
public sealed class ResourceSidebarItemOptions
{
    /// <summary>
    ///     Gets the label of the sidebar group the item appears in.
    /// </summary>
    /// <value>
    ///     The group label. The default is "Resources".
    /// </value>
    public string Group
    {
        get;
        internal set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value;
        }
    } = "Resources";

    /// <summary>
    ///     Gets the name of the icon shown before the item's label.
    /// </summary>
    /// <value>
    ///     The icon name, or <see langword="null" /> for no icon. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Icon
    {
        get;
        internal set
        {
            if (value is not null)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(value);
            }

            field = value;
        }
    }

    /// <summary>
    ///     Gets the item's label.
    /// </summary>
    /// <value>
    ///     The label, or <see langword="null" /> to use the resource's plural label. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Label
    {
        get;
        internal set
        {
            if (value is not null)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(value);
            }

            field = value;
        }
    }

    /// <summary>
    ///     Gets the item's position within its group.
    /// </summary>
    /// <value>
    ///     The position. Items with lower values come first. The default is 0.
    /// </value>
    public int Order { get; internal set; }

    /// <summary>
    ///     Gets a value that indicates whether the item appears in the sidebar.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the item appears in the sidebar; otherwise,
    ///     <see langword="false" />. The default is <see langword="true" />.
    /// </value>
    /// <remarks>
    ///     Hiding an item doesn't disable its pages.
    /// </remarks>
    public bool Visible { get; internal set; } = true;
}
