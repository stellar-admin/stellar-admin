using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.EntityFrameworkCore;

internal sealed class EfCoreLookupItems<TEntity, TValue>
    : ResourceReferenceItems,
        IEfCoreLookupReference
    where TEntity : class
{
    private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod(
        nameof(string.Contains),
        [typeof(string)]
    )!;
    private static readonly MethodInfo ValueContainsMethod = new Func<
        IEnumerable<TValue>,
        TValue,
        bool
    >(Enumerable.Contains).Method;
    private static readonly MethodInfo ToLowerMethod = typeof(string).GetMethod(
        nameof(string.ToLower),
        Type.EmptyTypes
    )!;

    private readonly Func<TEntity, string?>? _description;
    private readonly Func<TEntity, string?>? _media;
    private readonly EfCoreLookupItemsOptions<TEntity, TValue> _options;
    private readonly Expression<Func<TEntity, LookupProjection>> _projection;
    private readonly IReadOnlyList<PropertyInfo[]>? _readPaths;
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
        _readPaths = ReadPaths(options);
    }

    public override bool HasDescription => _description is not null;

    public override Type ItemType => typeof(TEntity);

    public override Type? MediaType => _options.Media?.Type;

    public override async Task<IReadOnlyList<ChoiceItem>> FindAsync(
        IServiceProvider services,
        FieldEditorContext context,
        CancellationToken cancellationToken
    )
    {
        var values = ReadValues(context);
        if (values.Count == 0)
        {
            return [];
        }

        var db = GetContext(services);

        // An edit page loads the reference with the entity; a changed selection no longer matches it
        if (
            values is [var current]
            && db.Model.FindEntityType(context.Model.GetType()) is { } model
            && FindLoadedNavigation(model, context.FieldName)?.PropertyInfo?.GetValue(context.Model)
                is TEntity reference
            && EqualityComparer<TValue>.Default.Equals(_value(reference), current)
        )
        {
            return
            [
                new ChoiceItem(FormatValue(current), FormatTitle(_title(reference), current))
                {
                    Description = _description?.Invoke(reference),
                    Media = CreateMedia(_media?.Invoke(reference)),
                },
            ];
        }

        var rows = await db.Set<TEntity>()
            .AsNoTracking()
            .Where(ValueIn(values))
            .Select(_projection)
            .ToListAsync(cancellationToken);

        // A value the entity set no longer has is still displayed, so the selection stays visible
        return values
            .Select(current =>
                rows.FirstOrDefault(row =>
                    EqualityComparer<TValue>.Default.Equals(row.Value, current)
                )
                    is { } row
                    ? CreateItem(row)
                    : new ChoiceItem(
                        FormatValue(current),
                        Convert.ToString(current, CultureInfo.InvariantCulture) ?? ""
                    )
            )
            .ToArray();
    }

    public IEnumerable<string> FindIncludePaths(IEntityType model, string fieldName)
    {
        if (FindLoadedNavigation(model, fieldName) is not { } navigation)
        {
            return [];
        }

        return _readPaths!
            .Select(path =>
                EfCorePropertyPaths.FindReferencePath(navigation.TargetEntityType, path)
            )
            .OfType<string>()
            .Select(path => $"{navigation.Name}.{path}")
            .Prepend(navigation.Name);
    }

    public override async Task<IReadOnlyList<ChoiceItem>> ListAsync(
        IServiceProvider services,
        object? model,
        CancellationToken cancellationToken
    )
    {
        var rows = await Query(services, model, null)
            .Select(_projection)
            .ToListAsync(cancellationToken);

        return rows.Select(CreateItem).ToArray();
    }

    public override async Task<LookupResults> SearchAsync(
        IServiceProvider services,
        LookupQuery query,
        CancellationToken cancellationToken
    )
    {
        var rows = await Query(services, query.Model, query.Term)
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

    // The property paths the selected item is created from, or null when a selector uses the entity
    // other than through its properties
    private static IReadOnlyList<PropertyInfo[]>? ReadPaths(
        EfCoreLookupItemsOptions<TEntity, TValue> options
    )
    {
        LambdaExpression?[] selectors =
        [
            options.ValueExpression,
            options.TitleExpression,
            options.DescriptionExpression,
            options.Media?.Selector,
        ];
        var paths = new List<PropertyInfo[]>();
        foreach (var selector in selectors.OfType<LambdaExpression>())
        {
            if (EfCorePropertyPaths.GetPropertyPaths(selector) is not { } selectorPaths)
            {
                return null;
            }

            paths.AddRange(selectorPaths);
        }

        return paths;
    }

    private static IReadOnlyList<TValue> ReadValues(FieldEditorContext context) =>
        context.Value switch
        {
            null => [],
            TValue current => [current],
            IEnumerable<TValue> values => values
                .Where(current => current is not null)
                .Distinct()
                .ToArray(),
            _ => throw new InvalidOperationException(
                $"The lookup on {context.FieldName} has a {context.Value.GetType().Name} value, but its items use {typeof(TValue).Name}."
            ),
        };

    // Selected values are posted with the form, which binds them in the current culture
    private static string FormatValue(TValue value) =>
        Convert.ToString(value, CultureInfo.CurrentCulture) ?? "";

    private ChoiceItem CreateItem(LookupProjection row) =>
        new(FormatValue(row.Value), FormatTitle(row.Title, row.Value))
        {
            Description = row.Description,
            Media = CreateMedia(row.Media),
        };

    private ItemMedia? CreateMedia(string? value) => _options.Media?.Create(value);

    // The reference that loads the selected item with the model, or null when the item's text reads
    // something an include can't load, so FindAsync queries the item instead
    private INavigation? FindLoadedNavigation(IEntityType model, string fieldName) =>
        _readPaths is not null
        && FindNavigation(model, fieldName) is { } navigation
        && _readPaths.All(path => EfCorePropertyPaths.CanInclude(navigation.TargetEntityType, path))
            ? navigation
            : null;

    private INavigation? FindNavigation(IEntityType model, string fieldName)
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

    private string FormatTitle(string? title, TValue value) =>
        string.IsNullOrEmpty(title) && _options.TitlesFallBackToValue
            ? Convert.ToString(value, CultureInfo.InvariantCulture) ?? ""
            : title ?? "";

    private DbContext GetContext(IServiceProvider services) =>
        EfCoreContexts.For(services).Get(services, typeof(TEntity), _options.ContextType);

    private Expression<Func<TEntity, bool>>? Matches(string term)
    {
        // A captured term is sent as a query parameter rather than inlined
        var lowered = term.ToLowerInvariant();
        Expression<Func<string>> parameter = () => lowered;
        var entity = Expression.Parameter(typeof(TEntity), "entity");
        if (_options.SearchExpressions.Count == 0)
        {
            if (_options.SearchPredicate is { } predicate)
            {
                return predicate(term);
            }

            if (!_options.CanQueryTitle)
            {
                return null;
            }
        }

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

    // The items the form's filters keep, matching a search term, in order
    private IOrderedQueryable<TEntity> Query(IServiceProvider services, object? model, string? term)
    {
        IQueryable<TEntity> entities = GetContext(services).Set<TEntity>().AsNoTracking();
        foreach (var formFilter in _options.FormFilters)
        {
            entities = entities.Where(formFilter(model));
        }

        if (!string.IsNullOrEmpty(term) && Matches(term) is { } matches)
        {
            entities = entities.Where(matches);
        }

        var ordered =
            _options.OrderQuery?.Invoke(entities)
            ?? (
                _options.CanQueryTitle
                    ? entities.OrderBy(_options.TitleExpression)
                    : entities.OrderBy(_options.ValueExpression)
            );

        return ordered.ThenBy(_options.ValueExpression);
    }

    private Expression<Func<TEntity, bool>> ValueIn(IReadOnlyList<TValue> values)
    {
        // Captured values are sent as one query parameter rather than inlined
        var array = values.ToArray();
        Expression<Func<TValue[]>> parameter = () => array;
        var entity = Expression.Parameter(typeof(TEntity), "entity");

        return Expression.Lambda<Func<TEntity, bool>>(
            Expression.Call(
                ValueContainsMethod,
                parameter.Body,
                Rebind(_options.ValueExpression, entity)
            ),
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
