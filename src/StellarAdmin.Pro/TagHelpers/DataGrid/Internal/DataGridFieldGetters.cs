using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace StellarAdmin.Pro.TagHelpers;

/// <summary>
///     Compiled property getters used to read field values off data grid row items, cached
///     for the application lifetime and keyed by (type, dotted property path). String
///     fields key on the item's runtime type; expression fields key on the declared type
///     the caller passes. The key spaces can overlap, but an overlapping entry compiles to
///     an identical getter, so sharing one cache is safe. A getter walks the property path
///     with a null check on every intermediate value that can be null, so a null anywhere
///     in the path reads as a null field value.
/// </summary>
internal static class DataGridFieldGetters
{
    private static readonly ConcurrentDictionary<
        (Type Type, string Field),
        Func<object, object?>
    > FieldGetters = new ConcurrentDictionary<(Type, string), Func<object, object?>>();

    /// <summary>
    ///     Reads a field named by string — a property name or a dotted path
    ///     (<c>Customer.Name</c>). The getter resolves each segment by reflection, the
    ///     first on the item's runtime type, so each concrete type in a polymorphic list
    ///     gets its own getter and a segment with no matching public property fails with a
    ///     clear message.
    /// </summary>
    public static object? GetValue(object item, string field)
    {
        return FieldGetters.GetOrAdd(
            (item.GetType(), field),
            static key => CreateFieldGetter(key.Type, key.Field)
        )(item);
    }

    /// <summary>
    ///     Reads a field selected by a <c>field-for</c> expression. The getter casts the
    ///     item to <paramref name="declaredType" /> (the lambda's parameter type) and reads
    ///     the already-extracted property chain, preserving the C# semantics of the
    ///     expression (explicit interface implementations, shadowed properties). An item
    ///     not assignable to the declared type fails the cast, which is a genuine usage
    ///     error. <paramref name="field" /> is the chain's dotted path, passed in because
    ///     the caller already has it.
    /// </summary>
    public static object? GetValue(
        object item,
        Type declaredType,
        string field,
        IReadOnlyList<PropertyInfo> propertyChain
    )
    {
        return FieldGetters.GetOrAdd(
            (declaredType, field),
            static (key, chain) => CreateFieldGetter(key.Type, chain),
            propertyChain
        )(item);
    }

    /// <summary>
    ///     Emits an <c>object</c>-typed expression reading the rest of the property chain
    ///     off <paramref name="instance" />, yielding <c>null</c> when an intermediate
    ///     value is null. Intermediates that cannot be null (non-nullable value types) are
    ///     read straight through without a check.
    /// </summary>
    private static Expression BuildChainAccess(
        Expression instance,
        IReadOnlyList<PropertyInfo> propertyChain,
        int index
    )
    {
        var access = Expression.Property(instance, propertyChain[index]);
        if (index == propertyChain.Count - 1)
        {
            return Expression.Convert(access, typeof(object));
        }

        if (access.Type.IsValueType && Nullable.GetUnderlyingType(access.Type) is null)
        {
            return BuildChainAccess(access, propertyChain, index + 1);
        }

        var value = Expression.Variable(access.Type);
        return Expression.Block(
            typeof(object),
            [value],
            Expression.Assign(value, access),
            Expression.Condition(
                Expression.Equal(value, Expression.Constant(null, value.Type)),
                Expression.Constant(null, typeof(object)),
                BuildChainAccess(value, propertyChain, index + 1)
            )
        );
    }

    private static Func<object, object?> CreateFieldGetter(Type type, string field)
    {
        var segments = field.Split('.');
        var propertyChain = new PropertyInfo[segments.Length];
        var containerType = type;
        for (var i = 0; i < segments.Length; i++)
        {
            propertyChain[i] =
                containerType.GetProperty(segments[i], BindingFlags.Public | BindingFlags.Instance)
                ?? throw new InvalidOperationException(
                    segments.Length == 1
                        ? $"The data grid field '{field}' does not match a public property on '{type}'."
                        : $"The data grid field '{field}' is not valid: '{segments[i]}' is "
                            + $"not a public property on '{containerType}'."
                );
            containerType = propertyChain[i].PropertyType;
        }

        return CreateFieldGetter(type, propertyChain);
    }

    private static Func<object, object?> CreateFieldGetter(
        Type type,
        IReadOnlyList<PropertyInfo> propertyChain
    )
    {
        var itemParameter = Expression.Parameter(typeof(object), "item");
        var body = BuildChainAccess(
            Expression.Convert(itemParameter, type),
            propertyChain,
            index: 0
        );

        return Expression.Lambda<Func<object, object?>>(body, itemParameter).Compile();
    }
}
