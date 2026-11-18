using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of an index layout that shows the resources in a table.
/// </summary>
public sealed class ResourceTableLayoutOptions : ResourceLayoutOptions
{
    /// <summary>
    ///     Gets the columns, in display order.
    /// </summary>
    public IReadOnlyList<DataGridColumnOptions> Columns => MutableColumns;

    /// <summary>
    ///     Gets the sort order used when the user hasn't sorted the table.
    /// </summary>
    /// <value>
    ///     The sort order, or <see langword="null" /> to use the data source's order. The default
    ///     is <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     The sort order must select a sortable column.
    /// </remarks>
    public ResourceSort? DefaultSort { get; internal set; }

    /// <summary>
    ///     Gets the paging configuration.
    /// </summary>
    /// <value>
    ///     The configuration, or <see langword="null" /> when the table isn't paged. The default is
    ///     <see langword="null" />.
    /// </value>
    public ResourcePagingOptions? Paging { get; internal set; }

    /// <summary>
    ///     Gets the configuration of rows that expand to show details.
    /// </summary>
    /// <value>
    ///     The configuration, or <see langword="null" /> when rows don't expand. The default is
    ///     <see langword="null" />.
    /// </value>
    public ResourceRowDetailsOptions? RowDetails { get; internal set; }

    internal ConfigurationSource? DefaultSortSource { get; set; }

    internal List<DataGridColumnOptions> MutableColumns { get; } = [];

    internal ResourceTableLayoutOptions(string key)
        : base(key) { }
}
