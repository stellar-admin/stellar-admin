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
