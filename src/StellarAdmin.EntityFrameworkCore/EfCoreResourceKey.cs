using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace StellarAdmin.EntityFrameworkCore;

internal static class EfCoreResourceKey
{
    // Matches the entity whose primary key is the resource ID; false when the ID is not a valid key value
    public static bool TryCreatePredicate<TEntity>(
        DbContext db,
        string id,
        out Expression<Func<TEntity, bool>> predicate
    )
        where TEntity : class
    {
        var property = db
            .Model.FindEntityType(typeof(TEntity))!
            .FindPrimaryKey()!
            .Properties.Single();
        if (!TryParseResourceId(id, property.ClrType, out var key))
        {
            predicate = null!;
            return false;
        }

        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        predicate = Expression.Lambda<Func<TEntity, bool>>(
            Expression.Equal(
                Expression.Property(parameter, property.Name),
                Expression.Constant(key, property.ClrType)
            ),
            parameter
        );

        return true;
    }

    // Matches the entities whose primary key is one of the resource IDs; IDs that are not valid key values match nothing
    public static Expression<Func<TEntity, bool>> CreatePredicate<TEntity>(
        DbContext db,
        IEnumerable<string> ids
    )
        where TEntity : class
    {
        var property = db
            .Model.FindEntityType(typeof(TEntity))!
            .FindPrimaryKey()!
            .Properties.Single();
        var keys = ids.Select(id =>
                TryParseResourceId(id, property.ClrType, out var key) ? key : null
            )
            .Where(key => key is not null)
            .ToArray();
        var typedKeys = Array.CreateInstance(property.ClrType, keys.Length);
        Array.Copy(keys, typedKeys, keys.Length);

        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        return Expression.Lambda<Func<TEntity, bool>>(
            Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Contains),
                [property.ClrType],
                Expression.Constant(typedKeys),
                Expression.Property(parameter, property.Name)
            ),
            parameter
        );
    }

    private static bool TryParseResourceId(string id, Type underlyingType, out object? parsedId)
    {
        if (
            underlyingType == typeof(int)
            && int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
        )
        {
            parsedId = number;
            return true;
        }
        if (
            underlyingType == typeof(long)
            && long.TryParse(
                id,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var longNumber
            )
        )
        {
            parsedId = longNumber;
            return true;
        }
        if (underlyingType == typeof(Guid) && Guid.TryParse(id, out var guid))
        {
            parsedId = guid;
            return true;
        }
        if (underlyingType == typeof(string))
        {
            parsedId = id;
            return true;
        }

        parsedId = null;
        return false;
    }
}
