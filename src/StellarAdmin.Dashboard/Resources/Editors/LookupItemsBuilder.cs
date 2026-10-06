namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures what each lookup item contains.
/// </summary>
public sealed class LookupItemsBuilder<TEntity>
{
    internal Func<TEntity, string?>? Description { get; private set; }

    internal Func<TEntity, LookupMedia>? Media { get; private set; }

    internal LookupMediaType? MediaType { get; private set; }

    /// <summary>
    ///     Displays an avatar beside each item's title, with the title's initials when the image URL is null.
    ///     Replaces a code.
    /// </summary>
    public void UseAvatar(Func<TEntity, string?> imageUrl)
    {
        ArgumentNullException.ThrowIfNull(imageUrl);

        Media = entity => new LookupMedia(LookupMediaType.Avatar, imageUrl(entity));
        MediaType = LookupMediaType.Avatar;
    }

    /// <summary>
    ///     Displays a short code beside each item's title. Replaces an avatar.
    /// </summary>
    public void UseCode(Func<TEntity, string?> code)
    {
        ArgumentNullException.ThrowIfNull(code);

        Media = entity => new LookupMedia(LookupMediaType.Code, code(entity));
        MediaType = LookupMediaType.Code;
    }

    /// <summary>
    ///     Displays secondary text below each item's title.
    /// </summary>
    public void UseDescription(Func<TEntity, string?> description)
    {
        ArgumentNullException.ThrowIfNull(description);

        Description = description;
    }
}
