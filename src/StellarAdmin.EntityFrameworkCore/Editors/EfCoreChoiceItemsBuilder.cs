using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Configures the choices that load from an entity set.
/// </summary>
/// <typeparam name="TEntity">The type of the entities.</typeparam>
/// <typeparam name="TValue">The type of the value that identifies an entity.</typeparam>
public sealed class EfCoreChoiceItemsBuilder<TEntity, TValue>
    where TEntity : class
{
    private readonly EfCoreChoiceItemsOptions<TEntity, TValue> _options;

    internal EfCoreChoiceItemsBuilder(EfCoreChoiceItemsOptions<TEntity, TValue> options)
    {
        _options = options;
    }

    /// <summary>
    ///     Orders the choices by an entity property.
    /// </summary>
    /// <typeparam name="TSort">The type of the property to order by.</typeparam>
    /// <param name="selector">An expression that selects the property to order by.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="selector" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Grouped choices keep this order within their group.
    /// </remarks>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> OrderBy<TSort>(
        Expression<Func<TEntity, TSort>> selector
    )
    {
        ArgumentNullException.ThrowIfNull(selector);

        _options.OrderQuery = query => query.OrderBy(selector);

        return this;
    }

    /// <summary>
    ///     Shows an avatar beside each choice's title.
    /// </summary>
    /// <param name="imageUrl">
    ///     An expression that selects the image URL, or <see langword="null" /> or an empty string
    ///     to show the initials of the choice's title.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="imageUrl" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Replaces any other media. Editors that can't display media ignore it.
    /// </remarks>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> UseAvatar(
        Expression<Func<TEntity, string?>> imageUrl
    )
    {
        ArgumentNullException.ThrowIfNull(imageUrl);

        _options.Media = EfCoreItemMedia<TEntity>.Avatar(imageUrl);

        return this;
    }

    /// <summary>
    ///     Shows a short code beside each choice's title.
    /// </summary>
    /// <param name="code">
    ///     An expression that selects the code, or <see langword="null" /> or an empty string to
    ///     show no media for the choice.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="code" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Replaces any other media. Editors that can't display media ignore it.
    /// </remarks>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> UseCode(
        Expression<Func<TEntity, string?>> code
    )
    {
        ArgumentNullException.ThrowIfNull(code);

        _options.Media = EfCoreItemMedia<TEntity>.Code(code);

        return this;
    }

    /// <summary>
    ///     Loads the choices with a database context.
    /// </summary>
    /// <typeparam name="TContext">The type of the database context.</typeparam>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     By default, the registered database context whose model maps the entity is used. Pick
    ///     one when several contexts map it.
    /// </remarks>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> UseDbContext<TContext>()
        where TContext : DbContext
    {
        _options.ContextType = typeof(TContext);

        return this;
    }

    /// <summary>
    ///     Shows secondary text below each choice's title.
    /// </summary>
    /// <param name="description">
    ///     An expression that selects the description, or <see langword="null" /> to show none.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="description" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Editors that can't display descriptions ignore it.
    /// </remarks>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> UseDescription(
        Expression<Func<TEntity, string?>> description
    )
    {
        ArgumentNullException.ThrowIfNull(description);

        _options.DescriptionExpression = description;

        return this;
    }

    /// <summary>
    ///     Groups the choices under headings.
    /// </summary>
    /// <param name="group">
    ///     An expression that selects a choice's group heading, or <see langword="null" /> for a
    ///     choice outside any group.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="group" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Editors that can't display groups ignore them.
    /// </remarks>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> UseGroup(
        Expression<Func<TEntity, string?>> group
    )
    {
        ArgumentNullException.ThrowIfNull(group);

        _options.GroupExpression = group;

        return this;
    }

    /// <summary>
    ///     Shows a registered icon beside each choice's title.
    /// </summary>
    /// <param name="iconName">
    ///     An expression that selects the icon name, or <see langword="null" /> or an empty string
    ///     to show no media for the choice.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="iconName" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Replaces any other media. Editors that can't display media ignore it.
    /// </remarks>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> UseIcon(
        Expression<Func<TEntity, string?>> iconName
    )
    {
        ArgumentNullException.ThrowIfNull(iconName);

        _options.Media = EfCoreItemMedia<TEntity>.Icon(iconName);

        return this;
    }

    /// <summary>
    ///     Shows a square image beside each choice's title.
    /// </summary>
    /// <param name="imageUrl">
    ///     An expression that selects the image URL, or <see langword="null" /> or an empty string
    ///     to show no media for the choice.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="imageUrl" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Replaces any other media. Editors that can't display media ignore it.
    /// </remarks>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> UseImage(
        Expression<Func<TEntity, string?>> imageUrl
    )
    {
        ArgumentNullException.ThrowIfNull(imageUrl);

        _options.Media = EfCoreItemMedia<TEntity>.Image(imageUrl);

        return this;
    }

    /// <summary>
    ///     Filters the choices by the form's values.
    /// </summary>
    /// <typeparam name="TModel">The type of the form's model.</typeparam>
    /// <param name="filter">
    ///     An expression that returns whether an entity is a choice, from the entity and the form's
    ///     model.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="filter" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Declare each field the filter reads with <c>DependsOn</c>: the form holds only those
    ///     fields' values, and the key on the edit form. Calling this more than once keeps the
    ///     choices that match every filter.
    /// </remarks>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> Where<TModel>(
        Expression<Func<TEntity, TModel, bool>> filter
    )
    {
        ArgumentNullException.ThrowIfNull(filter);

        _options.FormFilters.Add(EfCoreFormFilter.Create(filter));

        return this;
    }
}
