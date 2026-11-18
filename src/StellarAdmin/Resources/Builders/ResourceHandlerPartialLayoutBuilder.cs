using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures an index layout that renders items a handler loads with a partial view.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
/// <typeparam name="TItem">The type of the items the handler loads.</typeparam>
/// <remarks>
///     By default the items don't link anywhere, so
///     <see cref="ResourceLayoutModel{TItem}.GetViewUrl" /> and
///     <see cref="ResourceLayoutModel{TItem}.GetEditUrl" /> return <see langword="null" />.
///     <see cref="HasRecordKey" /> makes each item a record of the resource, and
///     <see cref="LinkTo{TOther}" /> links each item to another resource.
/// </remarks>
public sealed class ResourceHandlerPartialLayoutBuilder<TResource, TItem>
    : ResourceLayoutBuilderBase<TResource, ResourceHandlerPartialLayoutBuilder<TResource, TItem>>
{
    internal ResourceHandlerPartialLayoutBuilder(IServiceCollection services, string key)
        : base(services, key) { }

    /// <summary>
    ///     Makes each item a record of the resource, which links to the record's view and edit
    ///     pages.
    /// </summary>
    /// <param name="key">
    ///     A delegate that returns the key of the record an item is, or <see langword="null" /> for
    ///     an item that isn't a record.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="LinkTo{TOther}" /> was called on the layout.
    /// </exception>
    public ResourceHandlerPartialLayoutBuilder<TResource, TItem> HasRecordKey(
        Func<TItem, object?> key
    )
    {
        ConfigureRecordKey(key);

        return this;
    }

    /// <summary>
    ///     Links each item to the view page of another resource's record.
    /// </summary>
    /// <typeparam name="TOther">The type of the other resource.</typeparam>
    /// <param name="key">
    ///     A delegate that returns the key of the record an item links to, or
    ///     <see langword="null" /> for an item without a link.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The items have no edit link. They don't link when the other resource has no view page or
    ///     the user isn't allowed to open it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="HasRecordKey" /> was called on the layout.
    /// </exception>
    public ResourceHandlerPartialLayoutBuilder<TResource, TItem> LinkTo<TOther>(
        Func<TItem, object?> key
    )
    {
        ConfigureLink<TOther, TItem>(key);

        return this;
    }
}
