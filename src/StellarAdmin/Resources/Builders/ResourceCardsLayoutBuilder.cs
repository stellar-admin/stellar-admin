using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a cards layout of a resource's index page.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
public sealed class ResourceCardsLayoutBuilder<TResource>
    : ResourceLayoutBuilderBase<TResource, ResourceCardsLayoutBuilder<TResource>>
{
    internal ResourceCardsLayoutBuilder(IServiceCollection services, string key)
        : base(services, key) { }

    /// <summary>
    ///     Configures the parts of each card.
    /// </summary>
    /// <param name="configure">A delegate that configures the card.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A part that isn't set comes from the resource: the title from its title, and the
    ///     description and image from its reference.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceCardsLayoutBuilder<TResource> Card(
        Action<ResourceCardBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new(ConfigureCard));

        return this;
    }

    /// <summary>
    ///     Sorts the cards by a property in ascending order.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">An expression that selects the property to sort by.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    public ResourceCardsLayoutBuilder<TResource> DefaultSortBy<TProperty>(
        Expression<Func<TResource, TProperty>> field
    ) => ConfigureDefaultSort(field, ResourceSortDirection.Ascending);

    /// <summary>
    ///     Sorts the cards by a property in descending order.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">An expression that selects the property to sort by.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    public ResourceCardsLayoutBuilder<TResource> DefaultSortByDescending<TProperty>(
        Expression<Func<TResource, TProperty>> field
    ) => ConfigureDefaultSort(field, ResourceSortDirection.Descending);

    /// <summary>
    ///     Enables paging on the cards.
    /// </summary>
    /// <returns>A builder that configures paging.</returns>
    public ResourceCardsPagingBuilder<TResource> EnablePaging()
    {
        Services.Configure<ResourceOptions<TResource>>(options =>
            options.Index.GetCards(LayoutKey).Paging = new()
        );

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
    public ResourceCardsLayoutBuilder<TResource> EnablePaging(
        Action<ResourceCardsPagingBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnablePaging());

        return this;
    }

    /// <summary>
    ///     Configures which of the resource's row actions the cards show.
    /// </summary>
    /// <param name="configure">A delegate that turns row actions off.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The cards show an action only when the resource also allows it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceCardsLayoutBuilder<TResource> RowActions(
        Action<ResourceRowActionsBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new(Services, index => index.GetCards(LayoutKey).RowActions));

        return this;
    }

    private void ConfigureCard(Action<ResourceCardOptions> configure) =>
        Services.Configure<ResourceOptions<TResource>>(options =>
            configure(options.Index.GetCards(LayoutKey).Card)
        );

    private ResourceCardsLayoutBuilder<TResource> ConfigureDefaultSort(
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
                    $"The cards aren't sorted by `{field}` because it isn't a property path of the resource. The default sort is left unchanged.",
                    "Sort by a property of the resource, or a path of properties.",
                    source
                )
            );

            return this;
        }

        Services.Configure<ResourceOptions<TResource>>(options =>
        {
            var cards = options.Index.GetCards(LayoutKey);
            cards.DefaultSort = new(ResourcePropertyPath.GetName(properties), direction);
            cards.DefaultSortExpression = field;
            cards.DefaultSortSource = source;
        });

        return this;
    }
}
