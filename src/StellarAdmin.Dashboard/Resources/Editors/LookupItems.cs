using System.Globalization;
using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Supplies the items of a lookup editor. Integrations derive from it to search their own data.
/// </summary>
public abstract class LookupItems
{
    /// <summary>
    ///     Whether the items have a description.
    /// </summary>
    public abstract bool HasDescription { get; }

    /// <summary>
    ///     The type of the items, or null when it is unknown. It selects the resource that creates new items.
    /// </summary>
    public virtual Type? ItemType => null;

    /// <summary>
    ///     The type of media the items display, such as <see cref="ItemMedia.Avatar" />, or null when they have none.
    ///     The loading placeholders take its shape.
    /// </summary>
    public abstract Type? MediaType { get; }

    /// <summary>
    ///     Returns the selected items for the field's current value, which is one value or a collection of values, in
    ///     the order of the values and with each value formatted in the current culture. The list is empty when nothing
    ///     is selected.
    /// </summary>
    public abstract Task<IReadOnlyList<ChoiceItem>> FindAsync(
        IServiceProvider services,
        FieldEditorContext context,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///     Returns a page of items matching the query.
    /// </summary>
    public abstract Task<LookupResults> SearchAsync(
        IServiceProvider services,
        LookupQuery query,
        CancellationToken cancellationToken
    );
}

internal sealed class LookupItems<TSource, TEntity, TValue>(
    Func<TEntity, TValue> value,
    Func<TEntity, string> title,
    Func<TEntity, string?>? description,
    Func<TEntity, ItemMedia?>? media,
    Type? mediaType
) : LookupItems
    where TSource : class, ILookupSource<TEntity, TValue>
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

        var entities = await services
            .GetRequiredService<TSource>()
            .FindAsync(values, cancellationToken);

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
        var page = await services
            .GetRequiredService<TSource>()
            .SearchAsync(query, cancellationToken);

        return new LookupResults(page.Items.Select(CreateItem).ToArray(), page.HasMore);
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
                $"LookupSheetEditor on {context.FieldName} has a {context.Value.GetType().Name} value, but its items use {typeof(TValue).Name}."
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
