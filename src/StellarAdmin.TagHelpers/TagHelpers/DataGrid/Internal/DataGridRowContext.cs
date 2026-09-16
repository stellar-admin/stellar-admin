namespace StellarAdmin.TagHelpers;

/// <summary>
///     The state of a single row pass: the data item being rendered and the cells the columns
///     deposit for it. A fresh instance is installed on
///     <see cref="DataGridContext.CurrentRow" /> before each re-execution of the grid's child
///     content, so row state never leaks between passes.
/// </summary>
internal sealed class DataGridRowContext
{
    /// <summary>The data item for this row.</summary>
    public required object? Item { get; init; }

    /// <summary>The zero-based index of this row.</summary>
    public required int RowIndex { get; init; }

    /// <summary>Cells deposited by the columns during this row pass, in document order.</summary>
    public List<DataGridCell> Cells { get; } = [];
}
