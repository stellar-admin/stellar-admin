using System.Linq.Expressions;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures choices loaded from an EF Core entity set.
/// </summary>
public sealed class EfCoreChoiceItemsBuilder<TEntity, TValue>
    where TEntity : class
{
    private readonly EfCoreChoiceItemsOptions<TEntity, TValue> _options;

    internal EfCoreChoiceItemsBuilder(EfCoreChoiceItemsOptions<TEntity, TValue> options)
    {
        _options = options;
    }

    /// <summary>
    ///     Orders the choices by an entity property. Grouped choices keep this order within their group.
    /// </summary>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> OrderBy<TSort>(
        Expression<Func<TEntity, TSort>> selector
    )
    {
        ArgumentNullException.ThrowIfNull(selector);

        _options.OrderQuery = query => query.OrderBy(selector);

        return this;
    }

    /// <summary>
    ///     Displays an avatar beside each choice's text, in editors that display media, with the text's initials when
    ///     the image URL is null. Replaces other media.
    /// </summary>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> UseAvatar(
        Expression<Func<TEntity, string?>> imageUrl
    )
    {
        ArgumentNullException.ThrowIfNull(imageUrl);

        _options.Media = EfCoreItemMedia<TEntity>.Avatar(imageUrl);

        return this;
    }

    /// <summary>
    ///     Displays a short code beside each choice's text, in editors that display media. A choice with a null or
    ///     empty code has no media. Replaces other media.
    /// </summary>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> UseCode(
        Expression<Func<TEntity, string?>> code
    )
    {
        ArgumentNullException.ThrowIfNull(code);

        _options.Media = EfCoreItemMedia<TEntity>.Code(code);

        return this;
    }

    /// <summary>
    ///     Displays secondary text below each choice's text, in editors that display descriptions.
    /// </summary>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> UseDescription(
        Expression<Func<TEntity, string?>> description
    )
    {
        ArgumentNullException.ThrowIfNull(description);

        _options.DescriptionExpression = description;

        return this;
    }

    /// <summary>
    ///     Groups the choices by the selected heading, in editors that display groups. Choices with a null heading
    ///     are not grouped.
    /// </summary>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> UseGroup(
        Expression<Func<TEntity, string?>> group
    )
    {
        ArgumentNullException.ThrowIfNull(group);

        _options.GroupExpression = group;

        return this;
    }

    /// <summary>
    ///     Displays a registered icon beside each choice's text, in editors that display media. A choice with a null
    ///     or empty icon name has no media. Replaces other media.
    /// </summary>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> UseIcon(
        Expression<Func<TEntity, string?>> iconName
    )
    {
        ArgumentNullException.ThrowIfNull(iconName);

        _options.Media = EfCoreItemMedia<TEntity>.Icon(iconName);

        return this;
    }

    /// <summary>
    ///     Displays a square image beside each choice's text, in editors that display media. A choice with a null or
    ///     empty image URL has no media. Replaces other media.
    /// </summary>
    public EfCoreChoiceItemsBuilder<TEntity, TValue> UseImage(
        Expression<Func<TEntity, string?>> imageUrl
    )
    {
        ArgumentNullException.ThrowIfNull(imageUrl);

        _options.Media = EfCoreItemMedia<TEntity>.Image(imageUrl);

        return this;
    }
}
