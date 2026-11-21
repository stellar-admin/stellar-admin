using System.Linq.Expressions;
using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of an index data layout that shows the resources as cards.
/// </summary>
public sealed class ResourceCardsDataLayoutOptions : ResourceDataLayoutOptions
{
    /// <summary>
    ///     Gets the configuration of each card.
    /// </summary>
    public ResourceCardOptions Card { get; } = new();

    /// <summary>
    ///     Gets the sort order of the cards.
    /// </summary>
    /// <value>
    ///     The sort order, or <see langword="null" /> to use the data source's order. The default
    ///     is <see langword="null" />.
    /// </value>
    public ResourceSort? DefaultSort { get; internal set; }

    /// <summary>
    ///     Gets the paging configuration.
    /// </summary>
    /// <value>
    ///     The configuration, or <see langword="null" /> when the cards aren't paged. The default
    ///     is <see langword="null" />.
    /// </value>
    public ResourcePagingOptions? Paging { get; internal set; }

    /// <summary>
    ///     Gets which of the resource's row actions the cards show.
    /// </summary>
    public ResourceRowActionsOptions RowActions { get; } = new();

    // The property path the default sort orders by
    internal LambdaExpression? DefaultSortExpression { get; set; }

    internal ConfigurationSource? DefaultSortSource { get; set; }

    // Where a data layout with a handler was added
    internal ConfigurationSource? Source { get; set; }

    // Lists the cards of a data layout with a handler, or null for one that shows the resource's own rows
    internal ResourceTableLoader? Loader { get; set; }

    internal override IReadOnlyList<LambdaExpression> Fields =>
        [
            .. new[]
            {
                Card.TitleField,
                Card.DescriptionField,
                Card.Image?.Field,
            }.OfType<LambdaExpression>(),
            .. new[] { Card.Badge }
                .Concat(Card.Stats)
                .Concat(Card.Fields)
                .OfType<FormDisplayFieldOptions>()
                .Select(entry => entry.FieldExpression),
        ];

    internal ResourceCardsDataLayoutOptions(string key)
        : base(key) { }

    // The default sort with the expression to order by, or null when there's none
    internal ResourceSort? FindDefaultSort() =>
        DefaultSort is { } sort ? FindSort(sort.Field, sort.Direction) : null;

    // The sort the URL names, which for now can only be the default sort's field, in either direction. Null for any
    // other field
    internal ResourceSort? FindSort(string? field, ResourceSortDirection direction) =>
        DefaultSort is { } sort
        && DefaultSortExpression is { } selector
        && string.Equals(sort.Field, field, StringComparison.OrdinalIgnoreCase)
            ? new(sort.Field, direction) { Selector = selector }
            : null;
}
