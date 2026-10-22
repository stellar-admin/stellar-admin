using System.Globalization;
using System.Linq.Expressions;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources;

// How fields on other resources' forms refer to a resource: the items they pick from, and a record's title. It is
// created from the resource's options once they are built, as fields find it when their form renders.
internal sealed class ResourceReference
{
    private readonly Func<IReadOnlyList<LambdaExpression>, ResourceReferenceItems>? _createItems;
    private readonly Func<object, string>? _formatTitle;

    private ResourceReference(
        string? unavailableCause,
        string label,
        bool appliesFilters,
        Func<IReadOnlyList<LambdaExpression>, ResourceReferenceItems>? createItems,
        Func<object, string>? formatTitle
    )
    {
        UnavailableCause = unavailableCause;
        Label = label;
        AppliesFilters = appliesFilters;
        _createItems = createItems;
        _formatTitle = formatTitle;
    }

    // Whether the items apply the filters a field gives them; a data source can't
    public bool AppliesFilters { get; }

    // The resource's singular label, capitalized, which labels the fields that refer to it
    public string Label { get; }

    // Why fields can't refer to the resource, completing "because ..."
    public string? UnavailableCause { get; }

    public static ResourceReference Create<TResource>(ResourceOptions<TResource> options)
    {
        var name = typeof(TResource).Name.Split('`')[0];
        var label = options.SingularLabel is [var first, .. var rest]
            ? char.ToUpper(first, CultureInfo.CurrentCulture) + rest
            : options.SingularLabel;
        if (options.IsUnavailable)
        {
            return new($"the `{name}` resource is unavailable", label, false, null, null);
        }

        if (options.KeySelector is not { } key)
        {
            return new($"the `{name}` resource has no key", label, false, null, null);
        }

        // A record without a title is named by its key
        var title = options.Title?.Compile();
        string FormatTitle(TResource record) =>
            title?.Invoke(record) is { Length: > 0 } text ? text : key(record);

        return new(
            null,
            label,
            options.ReferenceItemsFactory is not null,
            options.ReferenceItemsFactory
                ?? (_ => new DataSourceReferenceItems<TResource>(options, FormatTitle)),
            record => FormatTitle((TResource)record)
        );
    }

    public ResourceReferenceItems CreateItems(IReadOnlyList<LambdaExpression> filters) =>
        _createItems is { } createItems
            ? createItems(filters)
            : throw new InvalidOperationException(
                $"Fields can't refer to the resource because {UnavailableCause}."
            );

    public string FormatTitle(object record) =>
        _formatTitle is { } formatTitle
            ? formatTitle(record)
            : throw new InvalidOperationException(
                $"Records can't be named because {UnavailableCause}."
            );
}
