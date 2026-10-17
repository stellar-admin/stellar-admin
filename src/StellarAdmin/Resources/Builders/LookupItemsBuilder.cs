using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures what each lookup item contains.
/// </summary>
public sealed class LookupItemsBuilder<TEntity>
{
    internal LookupItemsBuilder() { }

    internal Func<TEntity, string?>? Description { get; private set; }

    internal Func<TEntity, ItemMedia?>? Media { get; private set; }

    internal Type? MediaType { get; private set; }

    /// <summary>
    ///     Displays an avatar beside each item's title, with the title's initials when the image URL is null.
    ///     Replaces other media.
    /// </summary>
    public LookupItemsBuilder<TEntity> UseAvatar(Func<TEntity, string?> imageUrl)
    {
        ArgumentNullException.ThrowIfNull(imageUrl);

        UseMedia<ItemMedia.Avatar>(entity => new ItemMedia.Avatar(imageUrl(entity)));

        return this;
    }

    /// <summary>
    ///     Displays a short code beside each item's title. An item with a null or empty code has no media. Replaces
    ///     other media.
    /// </summary>
    public LookupItemsBuilder<TEntity> UseCode(Func<TEntity, string?> code)
    {
        ArgumentNullException.ThrowIfNull(code);

        UseMedia<ItemMedia.Code>(entity =>
            code(entity) is { Length: > 0 } value ? new ItemMedia.Code(value) : null
        );

        return this;
    }

    /// <summary>
    ///     Displays secondary text below each item's title.
    /// </summary>
    public LookupItemsBuilder<TEntity> UseDescription(Func<TEntity, string?> description)
    {
        ArgumentNullException.ThrowIfNull(description);

        Description = description;

        return this;
    }

    /// <summary>
    ///     Displays a registered icon beside each item's title. An item with a null or empty icon name has no media.
    ///     Replaces other media.
    /// </summary>
    public LookupItemsBuilder<TEntity> UseIcon(Func<TEntity, string?> iconName)
    {
        ArgumentNullException.ThrowIfNull(iconName);

        UseMedia<ItemMedia.Icon>(entity =>
            iconName(entity) is { Length: > 0 } name ? new ItemMedia.Icon(name) : null
        );

        return this;
    }

    /// <summary>
    ///     Displays a square image beside each item's title. An item with a null or empty image URL has no media.
    ///     Replaces other media.
    /// </summary>
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
