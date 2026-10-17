using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures what each lookup item shows.
/// </summary>
/// <typeparam name="TEntity">The type of the lookup's items.</typeparam>
public sealed class LookupItemsBuilder<TEntity>
{
    internal LookupItemsBuilder() { }

    internal Func<TEntity, string?>? Description { get; private set; }

    internal Func<TEntity, ItemMedia?>? Media { get; private set; }

    internal Type? MediaType { get; private set; }

    /// <summary>
    ///     Shows an avatar beside each item's title.
    /// </summary>
    /// <param name="imageUrl">
    ///     A function that returns an item's image URL, or <see langword="null" /> or an empty
    ///     string to show the initials of the item's title.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="imageUrl" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Replaces any other media.
    /// </remarks>
    public LookupItemsBuilder<TEntity> UseAvatar(Func<TEntity, string?> imageUrl)
    {
        ArgumentNullException.ThrowIfNull(imageUrl);

        UseMedia<ItemMedia.Avatar>(entity => new ItemMedia.Avatar(imageUrl(entity)));

        return this;
    }

    /// <summary>
    ///     Shows a short code beside each item's title.
    /// </summary>
    /// <param name="code">
    ///     A function that returns an item's code, or <see langword="null" /> or an empty string
    ///     to show no media for the item.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="code" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Replaces any other media.
    /// </remarks>
    public LookupItemsBuilder<TEntity> UseCode(Func<TEntity, string?> code)
    {
        ArgumentNullException.ThrowIfNull(code);

        UseMedia<ItemMedia.Code>(entity =>
            code(entity) is { Length: > 0 } value ? new ItemMedia.Code(value) : null
        );

        return this;
    }

    /// <summary>
    ///     Shows secondary text below each item's title.
    /// </summary>
    /// <param name="description">
    ///     A function that returns an item's description, or <see langword="null" /> to show none.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="description" /> is <see langword="null" />.
    /// </exception>
    public LookupItemsBuilder<TEntity> UseDescription(Func<TEntity, string?> description)
    {
        ArgumentNullException.ThrowIfNull(description);

        Description = description;

        return this;
    }

    /// <summary>
    ///     Shows a registered icon beside each item's title.
    /// </summary>
    /// <param name="iconName">
    ///     A function that returns an item's icon name, or <see langword="null" /> or an empty
    ///     string to show no media for the item.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="iconName" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Replaces any other media.
    /// </remarks>
    public LookupItemsBuilder<TEntity> UseIcon(Func<TEntity, string?> iconName)
    {
        ArgumentNullException.ThrowIfNull(iconName);

        UseMedia<ItemMedia.Icon>(entity =>
            iconName(entity) is { Length: > 0 } name ? new ItemMedia.Icon(name) : null
        );

        return this;
    }

    /// <summary>
    ///     Shows a square image beside each item's title.
    /// </summary>
    /// <param name="imageUrl">
    ///     A function that returns an item's image URL, or <see langword="null" /> or an empty
    ///     string to show no media for the item.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="imageUrl" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Replaces any other media.
    /// </remarks>
    public LookupItemsBuilder<TEntity> UseImage(Func<TEntity, string?> imageUrl)
    {
        ArgumentNullException.ThrowIfNull(imageUrl);

        UseMedia<ItemMedia.Image>(entity =>
            imageUrl(entity) is { Length: > 0 } url ? new ItemMedia.Image(url) : null
        );

        return this;
    }

    private void UseMedia<TMedia>(Func<TEntity, ItemMedia?> media)
        where TMedia : ItemMedia
    {
        Media = media;
        MediaType = typeof(TMedia);
    }
}
