namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures a resource's sidebar item.
/// </summary>
public sealed class ResourceSidebarItemOptions
{
    /// <summary>
    ///     The sidebar group label.
    /// </summary>
    /// <remarks>
    ///     Defaults to "Resources".
    /// </remarks>
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
    ///     The sidebar item label.
    /// </summary>
    /// <remarks>
    ///     Defaults to the resource's plural label.
    /// </remarks>
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
    ///     The item's order within its group.
    /// </summary>
    public int Order { get; internal set; }

    /// <summary>
    ///     Whether the item appears in the sidebar.
    /// </summary>
    /// <remarks>
    ///     Hiding an item does not disable its pages.
    /// </remarks>
    public bool Visible { get; internal set; } = true;
}
