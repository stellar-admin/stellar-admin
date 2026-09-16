using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

internal abstract class EfCoreReference<TEntity>
    where TEntity : class
{
    public abstract LambdaExpression DisplayExpression { get; }

    public abstract string FieldName { get; }

    public abstract string? CurrentLabel(IModel model, TEntity entity);

    public abstract string? CurrentValue(TEntity entity);

    public abstract IQueryable<TEntity> Include(IQueryable<TEntity> query);

    public abstract bool IsRequired(IModel model);

    public abstract Task<List<SelectListItem>> LoadChoices(
        DbContext db,
        CancellationToken cancellationToken
    );

    public abstract void Validate(IModel model);
}

internal sealed class EfCoreReference<TEntity, TKey, TTarget> : EfCoreReference<TEntity>
    where TEntity : class
    where TTarget : class
{
    private readonly ReferenceChoicesBuilder<TTarget> _choices;
    private readonly Func<TEntity, TKey> _getKey;
    private readonly Func<TTarget, string> _getLabel;
    private readonly Func<TEntity, TTarget?> _getTarget;
    private readonly Expression<Func<TTarget, string>> _label;
    private readonly Expression<Func<TEntity, TTarget?>> _navigation;

    public override LambdaExpression DisplayExpression { get; }

    public override string FieldName { get; }

    public EfCoreReference(
        string fieldName,
        Expression<Func<TEntity, TKey>> key,
        Expression<Func<TEntity, TTarget?>> navigation,
        Expression<Func<TTarget, string>> label,
        ReferenceChoicesBuilder<TTarget> choices
    )
    {
        FieldName = fieldName;
        _getKey = key.Compile();
        _navigation = navigation;
        _getTarget = navigation.Compile();
        _label = label;
        _getLabel = label.Compile();
        _choices = choices;

        var body = new ReplaceParameter(label.Parameters[0], navigation.Body).Visit(label.Body)!;
        DisplayExpression = Expression.Lambda(body, navigation.Parameters);
    }

    public override string? CurrentLabel(IModel model, TEntity entity)
    {
        var target = _getTarget(entity);

        var targetKey = target is null
            ? null
            : GetNavigation(model)
                .ForeignKey.PrincipalKey.Properties[0]
                .PropertyInfo!.GetValue(target);

        return
            target is null
            || Convert.ToString(targetKey, CultureInfo.InvariantCulture) != CurrentValue(entity)
            ? null
            : _getLabel(target);
    }

    public override string? CurrentValue(TEntity entity) =>
        Convert.ToString(_getKey(entity), CultureInfo.InvariantCulture);

    public override IQueryable<TEntity> Include(IQueryable<TEntity> query) =>
        query.Include(_navigation);

    public override bool IsRequired(IModel model) => GetNavigation(model).ForeignKey.IsRequired;

    public override async Task<List<SelectListItem>> LoadChoices(
        DbContext db,
        CancellationToken cancellationToken
    )
    {
        var navigation = GetNavigation(db.Model);
        var key = navigation.ForeignKey.PrincipalKey.Properties.Single();
        var parameter = _label.Parameters[0];
        var projection = Expression.Lambda<Func<TTarget, Choice>>(
            Expression.New(
                typeof(Choice).GetConstructors().Single(),
                Expression.Convert(
                    Expression.Property(parameter, key.PropertyInfo!),
                    typeof(object)
                ),
                _label.Body
            ),
            parameter
        );

        var rows = await _choices
            .Apply(db.Set<TTarget>().AsNoTracking(), _label)
            .Select(projection)
            .ToListAsync(cancellationToken);
        var items = rows.Select(row => new SelectListItem(
                row.Label,
                Convert.ToString(row.Key, CultureInfo.InvariantCulture)
            ))
            .ToList();
        items.Insert(
            0,
            new SelectListItem(
                navigation.ForeignKey.IsRequired ? "Select an option" : "Not set",
                ""
            )
        );

        return items;
    }

    public override void Validate(IModel model) => GetNavigation(model);

    private INavigation GetNavigation(IModel model)
    {
        var navigationName = ((MemberExpression)_navigation.Body).Member.Name;
        var navigation = model.FindEntityType(typeof(TEntity))?.FindNavigation(navigationName);
        var foreignKey = navigation?.ForeignKey;
        var principalKey = foreignKey?.PrincipalKey.Properties;
        var keyType = principalKey is { Count: 1 } ? principalKey[0].ClrType : null;

        if (
            navigation is null
            || navigation.IsCollection
            || !navigation.IsOnDependent
            || navigation.TargetEntityType.ClrType != typeof(TTarget)
            || foreignKey!.Properties.Count != 1
            || foreignKey.Properties[0].Name != FieldName
            || principalKey is not { Count: 1 }
            || principalKey[0].PropertyInfo is null
            || (
                keyType != typeof(int)
                && keyType != typeof(long)
                && keyType != typeof(Guid)
                && keyType != typeof(string)
            )
        )
        {
            throw new InvalidOperationException(
                $"Reference '{FieldName}' must match a single EF foreign key and dependent navigation with an int, long, Guid, or string principal key."
            );
        }

        return navigation;
    }

    private sealed record Choice(object Key, string Label);

    private sealed class ReplaceParameter(ParameterExpression parameter, Expression replacement)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == parameter ? replacement : base.VisitParameter(node);
    }
}
