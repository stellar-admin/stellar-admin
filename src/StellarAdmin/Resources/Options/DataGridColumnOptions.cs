using System.Linq.Expressions;
using Humanizer;
using StellarAdmin.Resources.Displays;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a data grid column.
/// </summary>
public sealed class DataGridColumnOptions
{
    /// <summary>
    ///     Gets the display that renders the column's cells.
    /// </summary>
    /// <value>
    ///     The display, or <see langword="null" /> to use the template the property's
    ///     <c>[UIHint]</c> names or the default display for its type.
    /// </value>
    public FieldDisplay? Display { get; internal set; }

    /// <summary>
    ///     Gets the text a cell shows when its value is empty.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use the property's
    ///     <c>[DisplayFormat(NullDisplayText)]</c> or an empty cell.
    /// </value>
    public string? EmptyText { get; internal set; }

    /// <summary>
    ///     Gets the name of the form field the column edits in an inline row.
    /// </summary>
    /// <value>
    ///     The field name, or <see langword="null" /> to match the field by
    ///     <see cref="FieldName" />.
    /// </value>
    public string? EditsFieldName { get; internal set; }

    /// <summary>
    ///     Gets the expression that selects the column's values.
    /// </summary>
    public LambdaExpression FieldExpression { get; }

    /// <summary>
    ///     Gets the property path that <see cref="FieldExpression" /> selects.
    /// </summary>
    /// <value>
    ///     The property path, or <see langword="null" /> when the expression doesn't select a
    ///     property path.
    /// </value>
    public string? FieldName { get; }

    /// <summary>
    ///     Gets a value that indicates whether the list can be sorted by the column.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the column's header sorts the list; otherwise,
    ///     <see langword="false" />.
    /// </value>
    public bool Sortable { get; internal set; }

    /// <summary>
    ///     Gets the expression that sorts the column.
    /// </summary>
    /// <value>
    ///     The expression, or <see langword="null" /> to sort by <see cref="FieldExpression" />.
    /// </value>
    public LambdaExpression? SortExpression { get; internal set; }

    /// <summary>
    ///     Gets the name that identifies the column when the list is sorted by it.
    /// </summary>
    /// <value>
    ///     <see cref="FieldName" />, or for a column that computes its value, its title in
    ///     PascalCase, such as <c>TagNames</c> for <c>Tag names</c>. <see langword="null" /> when
    ///     neither is set.
    /// </value>
    /// <remarks>
    ///     Sort links and <see cref="ResourceSort.Field" /> use this name.
    /// </remarks>
    public string? SortField =>
        FieldName
        ?? (
            string.IsNullOrWhiteSpace(Title)
                ? null
                : string.Concat(Title.Dehumanize().Where(char.IsLetterOrDigit))
        );

    /// <summary>
    ///     Gets the column's header text.
    /// </summary>
    /// <value>
    ///     The header text, or <see langword="null" /> to use the property's display name.
    /// </value>
    public string? Title { get; internal set; }

    internal bool UsesBuiltInRendering { get; set; }

    internal DataGridColumnOptions(LambdaExpression fieldExpression, string? fieldName)
    {
        FieldExpression = fieldExpression;
        FieldName = fieldName;
    }
}
