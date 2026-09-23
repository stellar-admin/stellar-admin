using System.Linq.Expressions;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     A configured column of a data grid.
/// </summary>
public sealed class DataGridColumnOptions
{
    /// <summary>The property used for cell values and sorting when it differs from the column field.</summary>
    public LambdaExpression? DisplayExpression { get; internal set; }

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
    ///     The composite format string applied to the column's cell values, or <c>null</c>
    ///     to resolve it from the bound property's metadata.
    /// </summary>
    public string? Format { get; internal set; }

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
    ///     The grid display template that renders the column's cells, or <c>null</c> to
    ///     resolve it from the bound property's metadata.
    /// </summary>
    public string? Template { get; internal set; }

    /// <summary>
    ///     The column header title, or <c>null</c> to derive it from the bound property's
    ///     metadata.
    /// </summary>
    public string? Title { get; internal set; }

    internal DataGridColumnOptions(LambdaExpression fieldExpression, string? fieldName)
    {
        FieldExpression = fieldExpression;
        FieldName = fieldName;
    }
}
