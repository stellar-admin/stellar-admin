using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

internal sealed class EfCoreResourceReferences<TEntity>
    where TEntity : class
{
    public List<EfCoreReference<TEntity>> Items { get; } = [];
}

internal abstract class EfCoreReference<TEntity>
    where TEntity : class
{
    public abstract LambdaExpression DisplayExpression { get; }

    public abstract string FieldName { get; }

    public abstract string NavigationName { get; }

    public abstract Task<IReadOnlyList<SelectListItem>> GetLookupsAsync(
        DbContext db,
        CancellationToken cancellationToken
    );

    public abstract void Validate(IEntityType entity);
}

internal sealed class EfCoreReference<TEntity, TTarget>(
    string fieldName,
    string navigationName,
    Expression<Func<TEntity, TTarget?>> navigation,
    Expression<Func<TTarget, string>> display
) : EfCoreReference<TEntity>
    where TEntity : class
    where TTarget : class
{
    private readonly Func<TTarget, string> _getLabel = display.Compile();

    public override LambdaExpression DisplayExpression { get; } =
        Expression.Lambda(
            Expression.Property(navigation.Body, ((MemberExpression)display.Body).Member.Name),
            navigation.Parameters
        );

    public override string FieldName => fieldName;

    public override string NavigationName => navigationName;

    public override async Task<IReadOnlyList<SelectListItem>> GetLookupsAsync(
        DbContext db,
        CancellationToken cancellationToken
    )
    {
        var foreignKey = GetNavigation(db.Model.FindEntityType(typeof(TEntity))!).ForeignKey;
        var principalKey = foreignKey.PrincipalKey.Properties.Single().PropertyInfo!;
        var targets = await db.Set<TTarget>()
            .AsNoTracking()
            .OrderBy(display)
            .ToListAsync(cancellationToken);
        List<SelectListItem> lookups =
        [
            new(foreignKey.IsRequired ? "Select an option" : "Not set", ""),
            .. targets.Select(target => new SelectListItem(
                _getLabel(target),
                Convert.ToString(principalKey.GetValue(target), CultureInfo.InvariantCulture)
            )),
        ];

        return lookups;
    }

    public override void Validate(IEntityType entity) => GetNavigation(entity);

    private INavigation GetNavigation(IEntityType entity)
    {
        var relation = entity.FindNavigation(navigationName);
        var foreignKey = relation?.ForeignKey;
        var principalKey = foreignKey?.PrincipalKey.Properties;
        var keyType = principalKey is { Count: 1 } ? principalKey[0].ClrType : null;
        if (
            relation is null
            || relation.IsCollection
            || !relation.IsOnDependent
            || relation.TargetEntityType.ClrType != typeof(TTarget)
            || foreignKey!.Properties.Count != 1
            || foreignKey.Properties[0].Name != fieldName
            || principalKey is not { Count: 1 }
            || principalKey[0].PropertyInfo is null
            || (
                Nullable.GetUnderlyingType(foreignKey.Properties[0].ClrType)
                ?? foreignKey.Properties[0].ClrType
            ) != keyType
            || (
                keyType != typeof(int)
                && keyType != typeof(long)
                && keyType != typeof(Guid)
                && keyType != typeof(string)
            )
        )
        {
            throw new InvalidOperationException(
                $"Reference '{fieldName}' must match a single EF foreign key and dependent navigation with an int, long, Guid, or string principal key."
            );
        }

        return relation;
    }
}
