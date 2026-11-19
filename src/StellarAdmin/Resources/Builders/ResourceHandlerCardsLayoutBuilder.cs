using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a cards layout of a resource's index page whose items a handler loads.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
/// <typeparam name="TItem">The type of the items the handler loads.</typeparam>
/// <remarks>
///     By default the cards don't link anywhere. <see cref="HasRecordKey" /> makes each card a
///     record of the resource, and <see cref="LinkTo{TOther}" /> links each card to another
///     resource.
/// </remarks>
public sealed class ResourceHandlerCardsLayoutBuilder<TResource, TItem>
    : ResourceLayoutBuilderBase<TResource, ResourceHandlerCardsLayoutBuilder<TResource, TItem>>
{
    internal ResourceHandlerCardsLayoutBuilder(IServiceCollection services, string key)
        : base(services, key) { }

    /// <summary>
    ///     Configures the parts of each card.
    /// </summary>
    /// <param name="configure">A delegate that configures the card.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A card shows only the parts it sets. Without a title, it shows the key that
    ///     <see cref="HasRecordKey" /> or <see cref="LinkTo{TOther}" /> returns; cards with
    ///     neither are a configuration problem.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceHandlerCardsLayoutBuilder<TResource, TItem> Card(
        Action<ResourceCardBuilder<TItem>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new(card => ConfigureCards(cards => card(cards.Card))));

        return this;
    }

    /// <summary>
    ///     Sorts the cards by a property in ascending order by default.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">An expression that selects the property to sort by.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The handler sorts the items: <see cref="ResourceListRequest.Sort" /> names the property.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    public ResourceHandlerCardsLayoutBuilder<TResource, TItem> DefaultSortBy<TProperty>(
        Expression<Func<TItem, TProperty>> field
    ) => ConfigureDefaultSort(field, ResourceSortDirection.Ascending);

    /// <summary>
    ///     Sorts the cards by a property in descending order by default.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">An expression that selects the property to sort by.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The handler sorts the items: <see cref="ResourceListRequest.Sort" /> names the property.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    public ResourceHandlerCardsLayoutBuilder<TResource, TItem> DefaultSortByDescending<TProperty>(
        Expression<Func<TItem, TProperty>> field
    ) => ConfigureDefaultSort(field, ResourceSortDirection.Descending);

    /// <summary>
    ///     Enables paging on the cards.
    /// </summary>
    /// <returns>A builder that configures paging.</returns>
    /// <remarks>
    ///     The handler pages the items: <see cref="ResourceListRequest.Paging" /> holds the page,
    ///     and the total it returns counts the pages.
    /// </remarks>
    public ResourceCardsPagingBuilder<TResource> EnablePaging()
    {
        ConfigureCards(cards => cards.Paging = new());

        return new(Services, LayoutKey);
    }

    /// <summary>
    ///     Enables and configures paging on the cards.
    /// </summary>
    /// <param name="configure">A delegate that configures paging.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceHandlerCardsLayoutBuilder<TResource, TItem> EnablePaging(
        Action<ResourceCardsPagingBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnablePaging());

        return this;
    }

    /// <summary>
    ///     Makes each card a record of the resource, with its view and edit links and the delete
    ///     action.
    /// </summary>
    /// <param name="key">
    ///     A delegate that returns the key of the record a card is, or <see langword="null" /> for
    ///     an item that isn't a record.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The cards get the row actions the resource allows, which <see cref="RowActions" /> can
    ///     turn off.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="LinkTo{TOther}" /> was called on the cards.
    /// </exception>
    public ResourceHandlerCardsLayoutBuilder<TResource, TItem> HasRecordKey(
        Func<TItem, object?> key
    )
    {
        ConfigureRecordKey(key);

        return this;
    }

    /// <summary>
    ///     Links each card to the view page of another resource's record.
    /// </summary>
    /// <typeparam name="TOther">The type of the other resource.</typeparam>
    /// <param name="key">
    ///     A delegate that returns the key of the record a card links to, or
    ///     <see langword="null" /> for a card without a link.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The cards have no other row actions. They don't link when the other resource has no view
    ///     page or the user isn't allowed to open it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="HasRecordKey" /> was called on the cards.
    /// </exception>
    public ResourceHandlerCardsLayoutBuilder<TResource, TItem> LinkTo<TOther>(
        Func<TItem, object?> key
    )
    {
        ConfigureLink<TOther, TItem>(key);

        return this;
    }

    /// <summary>
    ///     Configures which of the resource's row actions the cards show.
    /// </summary>
    /// <param name="configure">A delegate that turns row actions off.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Only cards that <see cref="HasRecordKey" /> makes records have row actions, and the
    ///     cards show an action only when the resource also allows it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceHandlerCardsLayoutBuilder<TResource, TItem> RowActions(
        Action<ResourceRowActionsBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new(Services, index => index.GetCards(LayoutKey).RowActions));

        return this;
    }

    private void ConfigureCards(Action<ResourceCardsLayoutOptions> configure) =>
        Services.Configure<ResourceOptions<TResource>>(options =>
            configure(options.Index.GetCards(LayoutKey))
        );

    private ResourceHandlerCardsLayoutBuilder<TResource, TItem> ConfigureDefaultSort(
        LambdaExpression field,
        ResourceSortDirection direction
    )
    {
        ArgumentNullException.ThrowIfNull(field);
        var source = ConfigurationSource.Capture();
        if (ResourcePropertyPath.GetProperties(field) is not { } properties)
        {
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Report(
                    ConfigurationProblemKind.InvalidSelector,
                    $"The cards aren't sorted by `{field}` because it isn't a property path of the item. The default sort is left unchanged.",
                    "Sort by a property of the item, or a path of properties.",
                    source
                )
            );

            return this;
        }

        ConfigureCards(cards =>
        {
            cards.DefaultSort = new(ResourcePropertyPath.GetName(properties), direction);
            cards.DefaultSortExpression = field;
            cards.DefaultSortSource = source;
        });

        return this;
    }
}
