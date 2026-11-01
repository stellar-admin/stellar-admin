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
        if (FindNamedProperty<TResource>() is { } named)
        {
            return named;
        }

        var type = typeof(TResource);
        if (keyPropertyName is null || type.GetProperty(keyPropertyName) is not { } key)
        {
            return null;
        }

        var record = Expression.Parameter(type, "record");
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

    // The string property [DisplayColumn] names, then a Name or Title property, leaving out the key
    public static Expression<Func<TResource, string?>>? FindNamedProperty<TResource>()
    {
        var type = typeof(TResource);
        var property =
            FindStringProperty(
                type,
                type.GetCustomAttribute<DisplayColumnAttribute>()?.DisplayColumn
            )
            ?? FindStringProperty(type, "Name")
            ?? FindStringProperty(type, "Title");
        if (property is null)
        {
            return null;
        }

        var record = Expression.Parameter(type, "record");

        return Expression.Lambda<Func<TResource, string?>>(
            Expression.Property(record, property),
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
