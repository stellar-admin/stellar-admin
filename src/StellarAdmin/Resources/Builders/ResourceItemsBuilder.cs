using System.Linq.Expressions;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the items a field takes from the resource it refers to.
/// </summary>
/// <typeparam name="TModel">The type of the form's model.</typeparam>
/// <typeparam name="TResource">The type of the resource.</typeparam>
public sealed class ResourceItemsBuilder<TModel, TResource>
{
    internal ResourceItemsBuilder() { }

    internal List<LambdaExpression> Filters { get; } = [];

    /// <summary>
    ///     Filters the items by the form's values.
    /// </summary>
    /// <param name="filter">
    ///     An expression that returns whether a record is an item, from the record and the form's
    ///     model.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="filter" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Only an EF Core resource applies the filter. Declare each field the filter reads with
    ///     <c>DependsOn</c>: the form holds only those fields' values, and the key on the edit form.
    ///     The filter applies to searches and choices, not to the selected items. Calling this more
    ///     than once keeps the items that match every filter.
    /// </remarks>
    public ResourceItemsBuilder<TModel, TResource> Where(
        Expression<Func<TResource, TModel, bool>> filter
    )
    {
        ArgumentNullException.ThrowIfNull(filter);

        Filters.Add(filter);

        return this;
    }
}
