using System.Linq.Expressions;

namespace StellarAdmin.Resources;

/// <summary>
///     Specifies how resources are sorted.
/// </summary>
/// <param name="Field">The name of the field to sort by.</param>
/// <param name="Direction">One of the enumeration values that specifies the sort direction.</param>
public sealed record ResourceSort(string Field, ResourceSortDirection Direction)
{
    /// <summary>
    ///     Gets the expression to order the resources by, such as a column's sort expression.
    /// </summary>
    /// <value>
    ///     The expression, which takes the resource as its parameter, or <see langword="null" />
    ///     when only <see cref="Field" /> is known. The default is <see langword="null" />.
    /// </value>
    public LambdaExpression? Selector { get; init; }
}
