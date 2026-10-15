namespace StellarAdmin.UI;

/// <summary>
///     Published by a data grid column before it executes its child content, so nested
///     template tags know which pass is running and where to deposit their captured content.
/// </summary>
internal sealed class DataGridColumnContext
{
    /// <summary>
    ///     The grid's row pass this column execution belongs to, or <c>null</c> during the
    ///     collect pass.
    /// </summary>
    public DataGridRowContext? CurrentRow { get; init; }

    /// <summary>True during the collect pass — derived from <see cref="CurrentRow" />.</summary>
    public bool Collecting => CurrentRow is null;

    /// <summary>
    ///     Whether this row pass renders the column's edit template instead of its display.
    /// </summary>
    public bool Editing { get; init; }

    /// <summary>Set during the collect pass when the column has an edit template.</summary>
    public bool HasEditTemplate { get; set; }

    public string? EditContent { get; set; }

    public string? HeaderContent { get; set; }

    public string? ItemContent { get; set; }
}
