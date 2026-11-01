using System.Linq.Expressions;
using System.Reflection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.EntityFrameworkCore;

// Where a create or edit form's model holds the entity: the model itself, or a property of a model that wraps it with
// fields of its own
internal sealed class EfCoreFormEntity
{
    public Type ModelType { get; }

    // The property that holds the entity, or null when the model is the entity
    public PropertyInfo? Property { get; }

    private EfCoreFormEntity(Type modelType, PropertyInfo? property)
    {
        ModelType = modelType;
        Property = property;
    }

    public static EfCoreFormEntity Self(Type entityType) => new(entityType, null);

    public static EfCoreFormEntity Wrapped<TModel, TEntity>(
        Expression<Func<TModel, TEntity>> entity
    )
    {
        if (
            entity.Body is not MemberExpression { Member: PropertyInfo property } member
            || member.Expression != entity.Parameters[0]
            || property is not { GetMethod.IsPublic: true, SetMethod.IsPublic: true }
        )
        {
            throw new ArgumentException(
                $"`{entity}` must select a property of `{typeof(TModel).Name}` with a public getter and setter.",
                nameof(entity)
            );
        }

        return new(typeof(TModel), property);
    }

    // A field's path from the entity, or null for a field of the wrapping model only
    public IReadOnlyList<PropertyInfo>? FindEntityPath(FormFieldOptions field) =>
        Property is null ? field.PropertyPath
        : field.PropertyPath.Count > 1 && field.PropertyPath[0].Name == Property.Name
            ? field.PropertyPath.Skip(1).ToArray()
        : null;

    public object GetEntity(object model) =>
        Property is null
            ? model
            : Property.GetValue(model)
                ?? throw new InvalidOperationException(
                    $"{ModelType.Name}.{Property.Name} is null. Initialize it, or set it with `UseFactory`."
                );

    public void SetEntity(object model, object entity) => Property?.SetValue(model, entity);
}
