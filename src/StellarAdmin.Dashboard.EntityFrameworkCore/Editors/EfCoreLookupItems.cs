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
    private readonly Func<TEntity, string?>? _media;
    private readonly EfCoreLookupItemsOptions<TEntity, TValue> _options;
    private readonly Expression<Func<TEntity, LookupProjection>> _projection;
    private readonly Func<TEntity, string> _title;
    private readonly Func<TEntity, TValue> _value;

    public EfCoreLookupItems(EfCoreLookupItemsOptions<TEntity, TValue> options)
    {
        _options = options;
        _value = options.ValueExpression.Compile();
        _title = options.TitleExpression.Compile();
        _description = options.DescriptionExpression?.Compile();
        _media = options.Media?.Selector.Compile();
        _projection = CreateProjection(options);
    }

    public override bool HasDescription => _description is not null;

    public override Type ItemType => typeof(TEntity);

    public override Type? MediaType => _options.Media?.Type;

    public override async Task<ChoiceItem?> FindAsync(
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
                    return new ChoiceItem(FormatValue(current), _title(reference))
                    {
                        Description = _description?.Invoke(reference),
                        Media = CreateMedia(_media?.Invoke(reference)),
                    };
                }

                var row = await db.Set<TEntity>()
                    .AsNoTracking()
                    .Where(ValueEquals(current))
                    .Select(_projection)
                    .FirstOrDefaultAsync(cancellationToken);

                // A value the entity set no longer has is still displayed, so the selection stays visible
                return row is null
                    ? new ChoiceItem(
                        FormatValue(current),
                        Convert.ToString(current, CultureInfo.InvariantCulture) ?? ""
                    )
                    : CreateItem(row);
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
            _options.OrderQuery?.Invoke(entities) ?? entities.OrderBy(_options.TitleExpression);
        var rows = await ordered
            .ThenBy(_options.ValueExpression)
            .Skip(query.Skip)
            .Take(query.Take + 1)
            .Select(_projection)
            .ToListAsync(cancellationToken);

        return new LookupResults(
            rows.Take(query.Take).Select(CreateItem).ToArray(),
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
            typeof(string),
        ])!;

        return Expression.Lambda<Func<TEntity, LookupProjection>>(
            Expression.New(
                constructor,
                Rebind(options.ValueExpression, entity),
                Rebind(options.TitleExpression, entity),
                RebindOrNull(options.DescriptionExpression, entity),
                RebindOrNull(options.Media?.Selector, entity)
            ),
            entity
        );
    }

    private static Expression Rebind(LambdaExpression selector, ParameterExpression entity) =>
        new ReplaceParameterVisitor(selector.Parameters[0], entity).Visit(selector.Body)!;

    private static Expression RebindOrNull(
        LambdaExpression? selector,
        ParameterExpression entity
    ) => selector is null ? Expression.Constant(null, typeof(string)) : Rebind(selector, entity);

    // Selected values are posted with the form, which binds them in the current culture
    private static string FormatValue(TValue value) =>
        Convert.ToString(value, CultureInfo.CurrentCulture) ?? "";

    private ChoiceItem CreateItem(LookupProjection row) =>
        new(FormatValue(row.Value), row.Title ?? "")
        {
            Description = row.Description,
            Media = CreateMedia(row.Media),
        };

    private ItemMedia? CreateMedia(string? value) => _options.Media?.Create(value);

    private Expression<Func<TEntity, bool>> Matches(string term)
    {
        // A captured term is sent as a query parameter rather than inlined
        var lowered = term.ToLowerInvariant();
        Expression<Func<string>> parameter = () => lowered;
        var entity = Expression.Parameter(typeof(TEntity), "entity");
        LambdaExpression[] selectors =
            _options.SearchExpressions.Count > 0
                ? [.. _options.SearchExpressions]
                : [_options.TitleExpression];
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

    private sealed record LookupProjection(
        TValue Value,
        string? Title,
        string? Description,
        string? Media
    );
}
