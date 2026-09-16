using System.Linq.Expressions;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     The configured default sort options for a data grid, applied when no explicit sort is requested.
/// </summary>
public sealed class DataGridDefaultSortOptions
{
    /// <summary>Whether the sort is descending.</summary>
    public bool Descending { get; }

    /// <summary>The expression selecting the property the data is sorted by.</summary>
    public LambdaExpression FieldExpression { get; }

    /// <summary>
    ///     The name of the property selected by <see cref="FieldExpression" />, or
    ///     <c>null</c> when the expression body is not a member access. Matched against
    ///     the sortable columns' fields to mark the column the default sort corresponds
    ///     to, if any, as the active sort.
    /// </summary>
    public string? FieldName { get; }

    internal DataGridDefaultSortOptions(
        LambdaExpression fieldExpression,
        string? fieldName,
        bool descending
    )
    {
        FieldExpression = fieldExpression;
        FieldName = fieldName;
        Descending = descending;
    }
}
