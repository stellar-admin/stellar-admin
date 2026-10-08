using System.Linq.Expressions;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

// The media an entity property selects: the property is projected with the query, and the media is created from its
// value. A code, icon or image without a value has no media; an avatar without an image shows initials.
internal sealed record EfCoreItemMedia<TEntity>(
    Expression<Func<TEntity, string?>> Selector,
    Type Type,
    Func<string?, ItemMedia?> Create
)
{
    public static EfCoreItemMedia<TEntity> Avatar(Expression<Func<TEntity, string?>> imageUrl) =>
        new(imageUrl, typeof(ItemMedia.Avatar), url => new ItemMedia.Avatar(url));

    public static EfCoreItemMedia<TEntity> Code(Expression<Func<TEntity, string?>> code) =>
        new(
            code,
            typeof(ItemMedia.Code),
            value => string.IsNullOrEmpty(value) ? null : new ItemMedia.Code(value)
        );

    public static EfCoreItemMedia<TEntity> Icon(Expression<Func<TEntity, string?>> iconName) =>
        new(
            iconName,
            typeof(ItemMedia.Icon),
            name => string.IsNullOrEmpty(name) ? null : new ItemMedia.Icon(name)
        );

    public static EfCoreItemMedia<TEntity> Image(Expression<Func<TEntity, string?>> imageUrl) =>
        new(
            imageUrl,
            typeof(ItemMedia.Image),
            url => string.IsNullOrEmpty(url) ? null : new ItemMedia.Image(url)
        );
}
