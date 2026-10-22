using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace StellarAdmin.Resources.Options;

// A record's title when the resource doesn't select one: the string property [DisplayColumn] names, then a Name or
// Title property, then the key
internal static class ResourceTitle
{
    public static Expression<Func<TResource, string?>>? FindConvention<TResource>(
        string? keyPropertyName
    )
    {
        var type = typeof(TResource);
        var record = Expression.Parameter(type, "record");
        var property =
            FindStringProperty(
                type,
                type.GetCustomAttribute<DisplayColumnAttribute>()?.DisplayColumn
            )
            ?? FindStringProperty(type, "Name")
            ?? FindStringProperty(type, "Title");
        if (property is not null)
        {
            return Expression.Lambda<Func<TResource, string?>>(
                Expression.Property(record, property),
                record
            );
        }

        if (keyPropertyName is null || type.GetProperty(keyPropertyName) is not { } key)
        {
            return null;
        }

        Expression keyValue = Expression.Property(record, key);

        return Expression.Lambda<Func<TResource, string?>>(
            key.PropertyType == typeof(string)
                ? keyValue
                : Expression.Call(
                    keyValue,
                    key.PropertyType.GetMethod(nameof(ToString), Type.EmptyTypes)!
                ),
            record
        );
    }

    private static PropertyInfo? FindStringProperty(Type type, string? name) =>
        name is not null
        && type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance)
            is { PropertyType: var propertyType, GetMethod.IsPublic: true } property
        && propertyType == typeof(string)
        && property.GetIndexParameters().Length == 0
            ? property
            : null;
}
