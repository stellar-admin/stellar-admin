using System.Linq.Expressions;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the parts of the cards of a cards layout.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
public sealed class ResourceCardBuilder<TResource>
{
    private readonly Action<Action<ResourceCardOptions>> _configure;

    /// <summary>
    ///     Sets where the row actions sit.
    /// </summary>
    /// <value>
    ///     The placement. The default is <see cref="ResourceCardActionsPlacement.Menu" />.
    /// </value>
    public ResourceCardActionsPlacement ActionsPlacement
    {
        set => _configure(options => options.ActionsPlacement = value);
    }

    /// <summary>
    ///     Sets how the image and the content are arranged.
    /// </summary>
    /// <value>
    ///     The arrangement. The default is <see cref="ResourceCardArrangement.Vertical" />.
    /// </value>
    /// <remarks>
    ///     Horizontal cards show the image above the content when the cards are narrow.
    ///     <see cref="ResourceCardArrangement.Overlay" /> needs an image; without one, the cards
    ///     are vertical.
    /// </remarks>
    public ResourceCardArrangement Arrangement
    {
        set
        {
            var source = ConfigurationSource.Capture();
            _configure(options =>
            {
                options.Arrangement = value;
                options.ArrangementSource = source;
            });
        }
    }

    internal ResourceCardBuilder(Action<Action<ResourceCardOptions>> configure) =>
        _configure = configure;

    /// <summary>
    ///     Shows a property as a badge.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">An expression that selects the property.</param>
    /// <returns>A builder that configures the badge.</returns>
    /// <remarks>
    ///     The badge uses <see cref="Displays.BadgeDisplay" /> unless
    ///     <see cref="ResourceCardBadgeBuilder.UseDisplay{TDisplay}()" /> sets another display. A
    ///     record whose value is empty shows no badge.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    public ResourceCardBadgeBuilder Badge<TProperty>(Expression<Func<TResource, TProperty>> field)
    {
        var badge = new ResourceCardBadgeBuilder(
            new ResourceCardEntryBuilder(ResourceDisplayFieldBuilder.Create(field, nameof(field)))
        );
        _configure(options => options.Badge = badge.Build());

        return badge;
    }

    /// <summary>
    ///     Shows a property as a badge, and configures it.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">An expression that selects the property.</param>
    /// <param name="configure">A delegate that configures the badge.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The badge uses <see cref="Displays.BadgeDisplay" /> unless
    ///     <see cref="ResourceCardBadgeBuilder.UseDisplay{TDisplay}()" /> sets another display. A
    ///     record whose value is empty shows no badge.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceCardBuilder<TResource> Badge<TProperty>(
        Expression<Func<TResource, TProperty>> field,
        Action<ResourceCardBadgeBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(Badge(field));

        return this;
    }

    /// <summary>
    ///     Sets the text under the title.
    /// </summary>
    /// <param name="description">An expression that selects a record's description.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="description" /> is <see langword="null" />.
    /// </exception>
    public ResourceCardBuilder<TResource> Description(
        Expression<Func<TResource, string?>> description
    )
    {
        ArgumentNullException.ThrowIfNull(description);
        _configure(options => options.DescriptionField = description);

        return this;
    }

    /// <summary>
    ///     Configures the fields, shown below the stats.
    /// </summary>
    /// <param name="configure">A delegate that adds the fields.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceCardBuilder<TResource> Fields(
        Action<ResourceCardFieldsBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(
            new(
                add => _configure(options => options.MutableFields.Add(add())),
                layout => _configure(options => options.FieldLayout = layout)
            )
        );

        return this;
    }

    /// <summary>
    ///     Shows an image on each card.
    /// </summary>
    /// <param name="imageUrl">An expression that selects a record's image URL.</param>
    /// <returns>A builder that configures the image.</returns>
    /// <remarks>
    ///     A record without an image shows a placeholder.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="imageUrl" /> is <see langword="null" />.
    /// </exception>
    public ResourceCardImageBuilder Image(Expression<Func<TResource, string?>> imageUrl)
    {
        ArgumentNullException.ThrowIfNull(imageUrl);
        _configure(options => options.Image = new(imageUrl));

        return new(configure => _configure(options => configure(options.Image!)));
    }

    /// <summary>
    ///     Shows an image on each card, and configures it.
    /// </summary>
    /// <param name="imageUrl">An expression that selects a record's image URL.</param>
    /// <param name="configure">A delegate that configures the image.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A record without an image shows a placeholder.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="imageUrl" /> or <paramref name="configure" /> is
    ///     <see langword="null" />.
    /// </exception>
    public ResourceCardBuilder<TResource> Image(
        Expression<Func<TResource, string?>> imageUrl,
        Action<ResourceCardImageBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(Image(imageUrl));

        return this;
    }

    /// <summary>
    ///     Configures the stats, a strip of short values shown above the fields.
    /// </summary>
    /// <param name="configure">A delegate that adds the stats.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A card shows up to four stats. More is a configuration problem, and the card shows the
    ///     first four.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceCardBuilder<TResource> Stats(
        Action<ResourceCardStatsBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new(add => _configure(options => options.MutableStats.Add(add()))));

        return this;
    }

    /// <summary>
    ///     Sets the card's title, which links to the record's view page.
    /// </summary>
    /// <param name="title">An expression that selects a record's title.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Without it, the card shows the resource's title, or the record's key when the resource
    ///     has no title.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="title" /> is <see langword="null" />.
    /// </exception>
    public ResourceCardBuilder<TResource> Title(Expression<Func<TResource, string?>> title)
    {
        ArgumentNullException.ThrowIfNull(title);
        _configure(options => options.TitleField = title);

        return this;
    }
}
