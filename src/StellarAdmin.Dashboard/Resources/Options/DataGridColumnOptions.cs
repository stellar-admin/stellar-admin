using System.Linq.Expressions;
using StellarAdmin.Dashboard.Resources.Displays;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     A configured column of a data grid.
/// </summary>
public sealed class DataGridColumnOptions
{
    /// <summary>The property used for cell values and sorting when it differs from the column field.</summary>
    public LambdaExpression? DisplayExpression { get; internal set; }

    /// <summary>
    ///     The display that renders the column's cells, or <c>null</c> to render the grid display
    ///     template the property's <c>[UIHint]</c> names, or the default display for its type.
    /// </summary>
    public FieldDisplay? Display { get; internal set; }

    /// <summary>
    ///     The text a cell shows when its value is empty, or <c>null</c> to use the property's
    ///     <c>[DisplayFormat(NullDisplayText)]</c>, then an empty cell.
    /// </summary>
    public string? EmptyText { get; internal set; }

    /// <summary>
    ///     The expression selecting the property that provides the column's cell
    ///     values.
    /// </summary>
    public LambdaExpression FieldExpression { get; }

    /// <summary>
    ///     The property path selected by <see cref="FieldExpression" />, or
    ///     <c>null</c> when the expression does not select a property path.
    /// </summary>
    public string? FieldName { get; }

    /// <summary>
    ///     Whether the column header renders a sort link and the column's field can be
    ///     passed as the containing page's sort field.
    /// </summary>
    public bool Sortable { get; internal set; }

    /// <summary>
    ///     The expression used to order the column, or null to use its field expression.
    /// </summary>
    public LambdaExpression? SortExpression { get; internal set; }

    /// <summary>
    ///     The column header title, or <c>null</c> to derive it from the bound property's
    ///     metadata.
    /// </summary>
    public string? Title { get; internal set; }

    internal bool UsesBuiltInRendering { get; set; }

    internal DataGridColumnOptions(LambdaExpression fieldExpression, string? fieldName)
    {
        FieldExpression = fieldExpression;
        FieldName = fieldName;
    }
}
