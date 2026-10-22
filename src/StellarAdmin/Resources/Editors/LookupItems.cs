using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Builders;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Supplies the items of a lookup editor.
/// </summary>
/// <remarks>
///     Integrations derive from this class to search their own data.
/// </remarks>
public abstract class LookupItems
{
    /// <summary>
    ///     Gets a value that indicates whether the items have a description.
    /// </summary>
    public abstract bool HasDescription { get; }

    /// <summary>
    ///     Gets the type of the items.
    /// </summary>
    /// <value>
    ///     The type, or <see langword="null" /> when it is unknown. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     When a lookup editor lets users create items, the resource registered for this type
    ///     creates them, unless the editor names a resource.
    /// </remarks>
    public virtual Type? ItemType => null;

    /// <summary>
    ///     Gets the type of media the items display.
    /// </summary>
    /// <value>
    ///     The media type, such as <see cref="ItemMedia.Avatar" />, or <see langword="null" /> when
    ///     the items have no media.
    /// </value>
    /// <remarks>
    ///     The placeholders shown while items load take the shape of this media.
    /// </remarks>
    public abstract Type? MediaType { get; }

    /// <summary>
    ///     Returns the selected items for the field's current value.
    /// </summary>
    /// <param name="services">The request's service provider.</param>
    /// <param name="context">The context of the field, which holds its current value.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    ///     The selected items in the order of the values, or an empty list when nothing is
    ///     selected.
    /// </returns>
    /// <remarks>
    ///     The field's value is one value or a collection of values. Format each item's value in
    ///     the current culture.
    /// </remarks>
    public abstract Task<IReadOnlyList<ChoiceItem>> FindAsync(
        IServiceProvider services,
        FieldEditorContext context,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///     Returns a page of items that match a query.
    /// </summary>
    /// <param name="services">The request's service provider.</param>
    /// <param name="query">The search term and the page of items to return.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The matching items, and whether more items follow them.</returns>
    public abstract Task<LookupResults> SearchAsync(
        IServiceProvider services,
        LookupQuery query,
        CancellationToken cancellationToken
    );

    internal static LookupItems Create<TSource, TEntity, TValue>(
        Func<TEntity, TValue> value,
        Func<TEntity, string> title,
        Action<LookupItemsBuilder<TEntity>> configure
    )
        where TSource : class, ILookupSource<TEntity, TValue>
    {
        var items = Build(value, title, configure);

        return new LookupItems<TSource, TEntity, TValue>(
            value,
            title,
            items.Description,
            items.Media,
            items.MediaType
        );
    }

    internal static LookupItems Create<TSource, TEntity, TValue, TArgument, TModel>(
        Func<TEntity, TValue> value,
        Func<TEntity, string> title,
        Func<TModel, TArgument> argument,
        Action<LookupItemsBuilder<TEntity>> configure,
        Type editorType
    )
        where TSource : class, ILookupSource<TEntity, TValue, TArgument>
    {
        ArgumentNullException.ThrowIfNull(argument);

        var items = Build(value, title, configure);

        return new LookupItems<TSource, TEntity, TValue, TArgument, TModel>(
            value,
            title,
            argument,
            items.Description,
            items.Media,
            items.MediaType,
            editorType
        );
    }

    private static LookupItemsBuilder<TEntity> Build<TEntity, TValue>(
        Func<TEntity, TValue> value,
        Func<TEntity, string> title,
        Action<LookupItemsBuilder<TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(configure);

        var items = new LookupItemsBuilder<TEntity>();
        configure(items);

        return items;
    }
}

// Items from a registered source, found by their values and searched by the source
internal abstract class SourceLookupItems<TEntity, TValue>(
    Func<TEntity, TValue> value,
    Func<TEntity, string> title,
    Func<TEntity, string?>? description,
    Func<TEntity, ItemMedia?>? media,
    Type? mediaType
) : LookupItems
{
    public override bool HasDescription => description is not null;

    public override Type ItemType => typeof(TEntity);

    public override Type? MediaType => mediaType;

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

        var entities = await FindEntitiesAsync(services, values, cancellationToken);

        // A value the source no longer has is still displayed, so the selection stays visible
        return values
            .Select(current =>
                entities.FirstOrDefault(entity =>
                    EqualityComparer<TValue>.Default.Equals(value(entity), current)
                )
                    is { } entity
                    ? CreateItem(entity)
                    : new ChoiceItem(
                        FormatValue(current),
                        Convert.ToString(current, CultureInfo.InvariantCulture) ?? ""
                    )
            )
            .ToArray();
    }

    public override async Task<LookupResults> SearchAsync(
        IServiceProvider services,
        LookupQuery query,
        CancellationToken cancellationToken
    )
    {
        var page = await SearchEntitiesAsync(services, query, cancellationToken);

        return new LookupResults(page.Items.Select(CreateItem).ToArray(), page.HasMore);
    }

    protected abstract Task<IReadOnlyCollection<TEntity>> FindEntitiesAsync(
        IServiceProvider services,
        IReadOnlyCollection<TValue> values,
        CancellationToken cancellationToken
    );

    protected abstract Task<LookupPage<TEntity>> SearchEntitiesAsync(
        IServiceProvider services,
        LookupQuery query,
        CancellationToken cancellationToken
    );

    private static IReadOnlyList<TValue> ReadValues(FieldEditorContext context) =>
        context.Value switch
        {
            null => [],
            TValue current => [current],
            IEnumerable<TValue> values => values
                .Where(current => current is not null)
                .Distinct()
                .ToArray(),
            _ => throw new ConfigurationException(
                ConfigurationProblemKind.UnsuitableEditor,
                $"its value is a `{context.Value.GetType().Name}`, but the lookup's items use `{typeof(TValue).Name}`",
                $"Make the lookup's items use the property's type, or bind the field to a `{typeof(TValue).Name}` property."
            ),
        };

    // Selected values are posted with the form, which binds them in the current culture
    private static string FormatValue(TValue current) =>
        Convert.ToString(current, CultureInfo.CurrentCulture) ?? "";

    private ChoiceItem CreateItem(TEntity entity) =>
        new(FormatValue(value(entity)), title(entity))
        {
            Description = description?.Invoke(entity),
            Media = media?.Invoke(entity),
        };
}

internal sealed class LookupItems<TSource, TEntity, TValue>(
    Func<TEntity, TValue> value,
    Func<TEntity, string> title,
    Func<TEntity, string?>? description,
    Func<TEntity, ItemMedia?>? media,
    Type? mediaType
) : SourceLookupItems<TEntity, TValue>(value, title, description, media, mediaType)
    where TSource : class, ILookupSource<TEntity, TValue>
{
    protected override Task<IReadOnlyCollection<TEntity>> FindEntitiesAsync(
        IServiceProvider services,
        IReadOnlyCollection<TValue> values,
        CancellationToken cancellationToken
    ) => services.GetRequiredService<TSource>().FindAsync(values, cancellationToken);

    protected override Task<LookupPage<TEntity>> SearchEntitiesAsync(
        IServiceProvider services,
        LookupQuery query,
        CancellationToken cancellationToken
    ) => services.GetRequiredService<TSource>().SearchAsync(query, cancellationToken);
}

// Items searched with a value the field takes from its form
internal sealed class LookupItems<TSource, TEntity, TValue, TArgument, TModel>(
    Func<TEntity, TValue> value,
    Func<TEntity, string> title,
    Func<TModel, TArgument> argument,
    Func<TEntity, string?>? description,
    Func<TEntity, ItemMedia?>? media,
    Type? mediaType,
    Type editorType
) : SourceLookupItems<TEntity, TValue>(value, title, description, media, mediaType)
    where TSource : class, ILookupSource<TEntity, TValue, TArgument>
{
    protected override Task<IReadOnlyCollection<TEntity>> FindEntitiesAsync(
        IServiceProvider services,
        IReadOnlyCollection<TValue> values,
        CancellationToken cancellationToken
    ) => services.GetRequiredService<TSource>().FindAsync(values, cancellationToken);

    protected override Task<LookupPage<TEntity>> SearchEntitiesAsync(
        IServiceProvider services,
        LookupQuery query,
        CancellationToken cancellationToken
    )
    {
        var model =
            query.Model
            ?? throw new InvalidOperationException(
                "The lookup's items are searched with a value from the form, but the search has no form."
            );

        return services
            .GetRequiredService<TSource>()
            .SearchAsync(
                argument(FormModel.Cast<TModel>(model, editorType)),
                query,
                cancellationToken
            );
    }
}
