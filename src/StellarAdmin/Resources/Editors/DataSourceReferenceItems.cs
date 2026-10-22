using System.Collections;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Editors;

// The items of fields that refer to a resource, loaded through its data source: selected records by their keys, and
// searches and choice lists in the order of the index's default sort. A data source applies no filters.
internal sealed class DataSourceReferenceItems<TResource> : ResourceReferenceItems
{
    private readonly Func<TResource, string?>? _description;
    private readonly Func<TResource, string> _key;
    private readonly Func<TResource, ItemMedia?>? _media;
    private readonly ResourceOptions<TResource> _options;
    private readonly Func<TResource, object?> _rawKey;
    private readonly Func<TResource, string> _title;

    public DataSourceReferenceItems(
        ResourceOptions<TResource> options,
        Func<TResource, string> title
    )
    {
        var keyProperty = typeof(TResource).GetProperty(options.KeyPropertyName!)!;
        var description = options.Reference.Description?.Compile();
        var media = options.Reference.Media?.Compile();
        var mediaType = options.Reference.MediaType;

        _options = options;
        _key = options.KeySelector!;
        _rawKey = record => keyProperty.GetValue(record);
        _title = title;
        _description = description;
        _media =
            media is null || mediaType is null
                ? null
                : record => ItemMediaFactory.Create(mediaType, media(record));
    }

    public override bool HasDescription => _description is not null;

    public override Type ItemType => typeof(TResource);

    public override Type? MediaType => _media is null ? null : _options.Reference.MediaType;

    public override async Task<IReadOnlyList<ChoiceItem>> FindAsync(
        IServiceProvider services,
        FieldEditorContext context,
        CancellationToken cancellationToken
    )
    {
        var values = ReadValues(context.Value);
        if (values.Count == 0)
        {
            return [];
        }

        var keys = values
            .Select(value => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "")
            .ToArray();
        var result = await GetDataSource(services)
            .ListAsync(new ResourceListRequest { Keys = keys }, cancellationToken);

        // A value the data source no longer has is still displayed, so the selection stays visible
        return values
            .Select(
                (value, index) =>
                    result.Items.FirstOrDefault(record => _key(record) == keys[index]) is { } record
                        ? CreateItem(record)
                        : new ChoiceItem(FormatValue(value), keys[index])
            )
            .ToArray();
    }

    public override async Task<IReadOnlyList<ChoiceItem>> ListAsync(
        IServiceProvider services,
        object? model,
        CancellationToken cancellationToken
    )
    {
        var result = await GetDataSource(services)
            .ListAsync(
                new ResourceListRequest { Sort = _options.Index.DefaultSort },
                cancellationToken
            );

        return result.Items.Select(CreateItem).ToArray();
    }

    public override async Task<LookupResults> SearchAsync(
        IServiceProvider services,
        LookupQuery query,
        CancellationToken cancellationToken
    )
    {
        // The sheet asks for the next page from the number of items it has
        var take = Math.Max(query.Take, 1);
        var result = await GetDataSource(services)
            .ListAsync(
                new ResourceListRequest
                {
                    Paging = new ResourcePaging(query.Skip / take + 1, take),
                    Search = query.Term,
                    Sort = _options.Index.DefaultSort,
                },
                cancellationToken
            );

        return new LookupResults(
            result.Items.Select(CreateItem).ToArray(),
            result.TotalCount > query.Skip + result.Items.Count
        );
    }

    private static IResourceDataSource<TResource> GetDataSource(IServiceProvider services) =>
        services.GetRequiredService<IResourceDataSource<TResource>>();

    // Selected values are posted with the form, which binds them in the current culture
    private static string FormatValue(object? value) =>
        Convert.ToString(value, CultureInfo.CurrentCulture) ?? "";

    private static IReadOnlyList<object> ReadValues(object? value) =>
        value switch
        {
            null => [],
            string text => [text],
            IEnumerable values => values.Cast<object?>().OfType<object>().Distinct().ToArray(),
            _ => [value],
        };

    private ChoiceItem CreateItem(TResource record) =>
        new(FormatValue(_rawKey(record)), _title(record))
        {
            Description = _description?.Invoke(record),
            Media = _media?.Invoke(record),
        };
}
