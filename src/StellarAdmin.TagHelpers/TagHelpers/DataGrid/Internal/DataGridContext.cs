using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Shared state between the data grid and its columns. A single mutable instance is
///     published before any child execution, so the collect pass and every row pass see the
///     same object.
/// </summary>
internal sealed class DataGridContext
{
    /// <summary>
    ///     The runtime type of the grid's data items (taken from the first non-null item), so
    ///     columns can read field metadata during the collect pass. <c>null</c> when the grid
    ///     has no items.
    /// </summary>
    public Type? ItemType { get; init; }

    /// <summary>
    ///     The row pass currently executing, or <c>null</c> during the first (collect) pass,
    ///     in which columns register their definitions and produce no output.
    /// </summary>
    public DataGridRowContext? CurrentRow { get; set; }

    /// <summary>
    ///     Grid display templates resolved by columns during the collect pass, keyed by
    ///     template name, so row passes render an already-resolved view instead of querying
    ///     the view engine per row.
    /// </summary>
    public Dictionary<string, IView> GridDisplayTemplates { get; } = [];

    /// <summary>
    ///     Field display metadata resolved by columns during the collect pass, keyed by field
    ///     name — the source of <c>[UIHint]</c> template hints and <c>[DisplayFormat]</c>
    ///     format strings. A <c>null</c> value records a field without resolvable metadata
    ///     (an empty grid, or a field with no matching property).
    /// </summary>
    public Dictionary<string, ModelMetadata?> FieldMetadata { get; } = [];

    /// <summary>True during the collect pass — derived from <see cref="CurrentRow" />.</summary>
    public bool Collecting => CurrentRow is null;

    /// <summary>Column definitions registered during the collect pass, in document order.</summary>
    public List<DataGridColumn> Columns { get; } = [];

    /// <summary>Custom empty-state content captured from <c>sa-data-grid-empty</c>.</summary>
    public string? EmptyContent { get; set; }

    /// <summary>
    ///     Rendered footer content (record-range summary and page links) captured from
    ///     <c>sa-data-grid-pager</c>, emitted in the grid's footer bar below the table.
    /// </summary>
    public string? PagerContent { get; set; }

    /// <summary>The sort declaration registered by <c>sa-data-grid-sort</c>.</summary>
    public DataGridSortTagHelper? Sort { get; set; }

    /// <summary>The row-selection declaration registered by <c>sa-data-grid-selection</c>.</summary>
    public DataGridSelection? Selection { get; set; }
}
