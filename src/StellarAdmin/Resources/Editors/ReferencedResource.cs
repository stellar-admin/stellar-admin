using System.Linq.Expressions;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Builders;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Identifies the resource a field takes its items from, with the field's filters.
/// </summary>
/// <remarks>
///     An editor stores the resource when it is configured, and gets the items from
///     <see cref="ResourceReferences" /> when its form renders.
/// </remarks>
public sealed class ReferencedResource
{
    private ReferencedResource(
        Type resourceType,
        IReadOnlyList<LambdaExpression> filters,
        ConfigurationSource? source
    )
    {
        ResourceType = resourceType;
        Filters = filters;
        Source = source;
    }

    /// <summary>
    ///     Gets the field's filters on the items.
    /// </summary>
    /// <value>
    ///     Expressions that take a record and the field's form, and return whether the record is an
    ///     item.
    /// </value>
    public IReadOnlyList<LambdaExpression> Filters { get; }

    /// <summary>
    ///     Gets the type of the resource.
    /// </summary>
    public Type ResourceType { get; }

    internal ConfigurationSource? Source { get; }

    /// <summary>
    ///     Returns the resource of a type.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <returns>The resource.</returns>
    public static ReferencedResource For<TResource>() =>
        new(typeof(TResource), [], ConfigurationSource.Capture());

    /// <summary>
    ///     Returns the resource of a type, with the items configured for a field.
    /// </summary>
    /// <typeparam name="TModel">The type of the form's model.</typeparam>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="configure">A delegate that configures the items.</param>
    /// <returns>The resource.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public static ReferencedResource For<TModel, TResource>(
        Action<ResourceItemsBuilder<TModel, TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        var items = new ResourceItemsBuilder<TModel, TResource>();
        configure(items);

        return new(typeof(TResource), items.Filters.ToArray(), ConfigurationSource.Capture());
    }

    internal static ReferencedResource Create(Type resourceType, ConfigurationSource? source) =>
        new(resourceType, [], source);
}
