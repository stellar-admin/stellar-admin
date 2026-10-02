using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

internal sealed class EfCoreLookupItems<TContext, TEntity, TValue>
    : LookupItems,
        IEfCoreLookupReference
    where TContext : DbContext
    where TEntity : class
{
    private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod(
        nameof(string.Contains),
        [typeof(string)]
    )!;
    private static readonly MethodInfo ToLowerMethod = typeof(string).GetMethod(
        nameof(string.ToLower),
        Type.EmptyTypes
    )!;

    private readonly Func<TEntity, string?>? _description;
    private readonly EfCoreLookupItemsOptions<TEntity, TValue> _options;
    private readonly Expression<Func<TEntity, LookupProjection>> _projection;
    private readonly Func<TEntity, string> _text;
    private readonly Func<TEntity, TValue> _value;

    public EfCoreLookupItems(EfCoreLookupItemsOptions<TEntity, TValue> options)
    {
        _options = options;
        _value = options.ValueExpression.Compile();
        _text = options.TextExpression.Compile();
        _description = options.DescriptionExpression?.Compile();
        _projection = CreateProjection(options);
    }

    public override async Task<LookupItem?> FindAsync(
        IServiceProvider services,
        FieldEditorContext context,
        CancellationToken cancellationToken
    )
    {
        switch (context.Value)
        {
            case null:
                return null;
            case TValue current:
                var db = services.GetRequiredService<TContext>();

                // An edit page loads the reference with the entity; a changed selection no longer matches it
                if (
                    db.Model.FindEntityType(context.Model.GetType()) is { } model
                    && FindNavigation(model, context.FieldName)
                        ?.PropertyInfo?.GetValue(context.Model)
                        is TEntity reference
                    && EqualityComparer<TValue>.Default.Equals(_value(reference), current)
                )
                {
                    return new LookupItem(_text(reference), _description?.Invoke(reference));
                }

                var row = await db.Set<TEntity>()
                    .AsNoTracking()
                    .Where(ValueEquals(current))
                    .Select(_projection)
                    .FirstOrDefaultAsync(cancellationToken);

                // A value the entity set no longer has is still displayed, so the selection stays visible
                return row is null
                    ? new LookupItem(
                        Convert.ToString(current, CultureInfo.InvariantCulture) ?? "",
                        null
                    )
                    : new LookupItem(row.Text ?? "", row.Description);
            default:
                throw new InvalidOperationException(
                    $"LookupEditor on {context.FieldName} has a {context.Value.GetType().Name} value, but its items use {typeof(TValue).Name}."
                );
        }
    }

    public INavigation? FindNavigation(IEntityType model, string fieldName)
    {
        if (_options.Reference is { } reference)
        {
            return
                model.ClrType == reference.ModelType
                && model.FindNavigation(reference.Navigation) is { IsCollection: false } navigation
                && navigation.TargetEntityType.ClrType == typeof(TEntity)
                ? navigation
                : null;
        }

        var foreignKeys = model
            .FindProperty(fieldName)
            ?.GetContainingForeignKeys()
            .Where(foreignKey =>
                foreignKey.Properties.Count == 1
                && foreignKey.PrincipalEntityType.ClrType == typeof(TEntity)
                && foreignKey.DependentToPrincipal is not null
            )
            .ToArray();

        return foreignKeys is [var single] ? single.DependentToPrincipal : null;
    }

    public override async Task<LookupResults> SearchAsync(
        IServiceProvider services,
        LookupQuery query,
        CancellationToken cancellationToken
    )
    {
        IQueryable<TEntity> entities = services
            .GetRequiredService<TContext>()
            .Set<TEntity>()
            .AsNoTracking();
        if (!string.IsNullOrEmpty(query.Term))
        {
            entities = entities.Where(Matches(query.Term));
        }

        var ordered =
            _options.OrderQuery?.Invoke(entities) ?? entities.OrderBy(_options.TextExpression);
        var rows = await ordered
            .ThenBy(_options.ValueExpression)
            .Skip(query.Skip)
            .Take(query.Take + 1)
            .Select(_projection)
            .ToListAsync(cancellationToken);

        // Selected values are posted with the form, which binds them in the current culture
        return new LookupResults(
            rows.Take(query.Take)
                .Select(row => new LookupResult(
                    Convert.ToString(row.Value, CultureInfo.CurrentCulture) ?? "",
                    row.Text ?? "",
                    row.Description
                ))
                .ToArray(),
            rows.Count > query.Take
        );
    }

    private static Expression<Func<TEntity, LookupProjection>> CreateProjection(
        EfCoreLookupItemsOptions<TEntity, TValue> options
    )
    {
        var entity = Expression.Parameter(typeof(TEntity), "entity");
        var constructor = typeof(LookupProjection).GetConstructor([
            typeof(TValue),
            typeof(string),
            typeof(string),
        ])!;

        return Expression.Lambda<Func<TEntity, LookupProjection>>(
            Expression.New(
                constructor,
                Rebind(options.ValueExpression, entity),
                Rebind(options.TextExpression, entity),
                options.DescriptionExpression is { } description
                    ? Rebind(description, entity)
                    : Expression.Constant(null, typeof(string))
            ),
            entity
        );
    }

    private static Expression Rebind(LambdaExpression selector, ParameterExpression entity) =>
        new ReplaceParameterVisitor(selector.Parameters[0], entity).Visit(selector.Body)!;

    private Expression<Func<TEntity, bool>> Matches(string term)
    {
        // A captured term is sent as a query parameter rather than inlined
        var lowered = term.ToLowerInvariant();
        Expression<Func<string>> parameter = () => lowered;
        var entity = Expression.Parameter(typeof(TEntity), "entity");
        LambdaExpression[] selectors =
            _options.SearchExpressions.Count > 0
                ? [.. _options.SearchExpressions]
                : [_options.TextExpression];
        var body = selectors
            .Select(selector =>
                (Expression)
                    Expression.Call(
                        Expression.Call(Rebind(selector, entity), ToLowerMethod),
                        ContainsMethod,
                        parameter.Body
                    )
            )
            .Aggregate(Expression.OrElse);

        return Expression.Lambda<Func<TEntity, bool>>(body, entity);
    }

    private Expression<Func<TEntity, bool>> ValueEquals(TValue current)
    {
        Expression<Func<TValue>> parameter = () => current;
        var entity = Expression.Parameter(typeof(TEntity), "entity");

        return Expression.Lambda<Func<TEntity, bool>>(
            Expression.Equal(Rebind(_options.ValueExpression, entity), parameter.Body),
            entity
        );
    }

    private sealed record LookupProjection(TValue Value, string? Text, string? Description);
}
