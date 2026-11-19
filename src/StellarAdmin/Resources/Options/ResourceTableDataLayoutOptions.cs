using System.Linq.Expressions;
using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of an index data layout that shows the resources in a table.
/// </summary>
public sealed class ResourceTableDataLayoutOptions : ResourceDataLayoutOptions
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
    ///     Gets which of the resource's row actions the table shows.
    /// </summary>
    public ResourceRowActionsOptions RowActions { get; } = new();

    /// <summary>
    ///     Gets the configuration of rows that expand to show details.
    /// </summary>
    /// <value>
    ///     The configuration, or <see langword="null" /> when rows don't expand. The default is
    ///     <see langword="null" />.
    /// </value>
    public ResourceRowDetailsOptions? RowDetails { get; internal set; }

    internal ConfigurationSource? DefaultSortSource { get; set; }

    internal override IReadOnlyList<LambdaExpression> Fields =>
        [.. Columns.Select(column => column.FieldExpression)];

    // Lists the rows of a table with a handler, or null for one that shows the resource's own rows
    internal ResourceTableLoader? Loader { get; set; }

    internal List<DataGridColumnOptions> MutableColumns { get; } = [];

    // The columns the list can be sorted by
    internal IEnumerable<DataGridColumnOptions> SortableColumns =>
        Columns.Where(column => column.Sortable);

    internal ResourceTableDataLayoutOptions(string key)
        : base(key) { }

    // The default sort with the expression to order by, or null when there's none or its column isn't sortable
    internal ResourceSort? FindDefaultSort() =>
        DefaultSort is { } sort ? FindSort(sort.Field, sort.Direction) : null;

    // The sort by the sortable column the field names, ignoring case, with the expression to order by: the column's
    // sort expression, or else its field expression. Null when no sortable column has the name
    internal ResourceSort? FindSort(string? field, ResourceSortDirection direction) =>
        field is not null
        && SortableColumns.FirstOrDefault(column =>
            string.Equals(column.SortField, field, StringComparison.OrdinalIgnoreCase)
        )
            is { } column
            ? new(column.SortField!, direction)
            {
                Selector = column.SortExpression ?? column.FieldExpression,
            }
            : null;
}
