using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;
using FrameworkAnchorTagHelper = Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper;

namespace StellarAdmin.UI;

/// <summary>
///     Configures the sort links of a data grid's sortable columns and the current sort.
/// </summary>
/// <remarks>
///     <para>
///         Set the link destination with the routing attributes, such as <c>asp-page</c> and
///         <c>asp-route-*</c>. The route values must contain the <c>{sortField}</c> placeholder,
///         which each link replaces with its column's sort field, and the
///         <c>{sortDirection}</c> placeholder, which it replaces with <c>asc</c> or <c>desc</c>.
///     </para>
///     <para>
///         Renders nothing itself. Columns show a sort link when they set <c>sortable</c>.
///     </para>
/// </remarks>
[HtmlTargetElement(
    "sa-data-grid-sort",
    ParentTag = "sa-data-grid",
    TagStructure = TagStructure.WithoutEndTag
)]
public sealed class DataGridSortTagHelper : StellarAdminAnchorTagHelperBase
{
    private const string SortFieldPlaceholder = "{sortField}";
    private const string SortDirectionPlaceholder = "{sortDirection}";

    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataGridSortTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders the sort links.</param>
    /// <param name="iconOptions">The icon options that supply the sort icons.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public DataGridSortTagHelper(IHtmlGenerator htmlGenerator, IOptions<IconOptions> iconOptions)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Gets or sets the field the data is sorted by.
    /// </summary>
    /// <remarks>
    ///     The column with this sort field, ignoring case, shows as sorted. Leave it out when the
    ///     data is unsorted.
    /// </remarks>
    [HtmlAttributeName("sort-field")]
    public string? SortField { get; set; }

    /// <summary>
    ///     Gets or sets the direction the data is sorted in.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the sort direction. The default is
    ///     <see cref="DataGridSortDirection.Ascending" />.
    /// </value>
    [HtmlAttributeName("sort-direction")]
    public DataGridSortDirection? SortDirection { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the links keep the current query string.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the links keep the current request's query string
    ///     parameters; otherwise, <see langword="false" />. The default is
    ///     <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     <c>asp-route-*</c> values take precedence over query string parameters with the same
    ///     name.
    /// </remarks>
    [HtmlAttributeName("preserve-query")]
    public bool? PreserveQuery { get; set; }

    /// <summary>
    ///     Gets or sets the query string parameters that <c>preserve-query</c> leaves out of the
    ///     links.
    /// </summary>
    /// <value>
    ///     A comma-separated list of parameter names, such as <c>pageNo</c>.
    /// </value>
    /// <remarks>
    ///     Use it for parameters that a new sort resets, such as the page number.
    /// </remarks>
    [HtmlAttributeName("preserve-query-except")]
    public string? PreserveQueryExcept { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     No route value contains <c>{sortField}</c>, or no route value contains
    ///     <c>{sortDirection}</c>.
    /// </exception>
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.SuppressOutput();

        var gridContext = GetContext<DataGridContext>(context);
        if (gridContext is not { Collecting: true })
        {
            // The data grid re-executes its child content once per row; the sort declaration
            // only registers during the collect pass.
            return Task.CompletedTask;
        }

        if (PreserveQuery ?? false)
        {
            var excludedKeys =
                PreserveQueryExcept?.Split(
                    ',',
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
                ) ?? [];
            foreach (var parameter in ViewContext.HttpContext.Request.Query)
            {
                if (excludedKeys.Contains(parameter.Key, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!RouteValues.ContainsKey(parameter.Key))
                {
                    RouteValues[parameter.Key] = parameter.Value.ToString();
                }
            }
        }

        if (!RouteValues.Any(value => value.Value?.Contains(SortFieldPlaceholder) == true))
        {
            throw new InvalidOperationException(
                "<sa-data-grid-sort> requires a route value containing the {sortField} sort field "
                    + "placeholder, e.g. asp-route-sortField=\"{sortField}\"."
            );
        }

        if (!RouteValues.Any(value => value.Value?.Contains(SortDirectionPlaceholder) == true))
        {
            throw new InvalidOperationException(
                "<sa-data-grid-sort> requires a route value containing the {sortDirection} sort "
                    + "direction placeholder, e.g. asp-route-sortDirection=\"{sortDirection}\"."
            );
        }

        gridContext.Sort = this;
        return Task.CompletedTask;
    }

    /// <summary>
    ///     The current sort direction of the given column sort field, or <c>null</c> when the
    ///     data is not sorted by it.
    /// </summary>
    internal DataGridSortDirection? GetActiveDirection(string sortField)
    {
        return string.Equals(sortField, SortField, StringComparison.OrdinalIgnoreCase)
            ? SortDirection ?? DataGridSortDirection.Ascending
            : null;
    }

    /// <summary>
    ///     Renders a column's header sort link: the header content followed by a sort
    ///     indicator icon, wrapped in a ghost-button anchor targeting the column's sort URL.
    /// </summary>
    internal async Task<IHtmlContent> RenderSortLink(
        TagHelperContext context,
        string sortField,
        IHtmlContent headerContent
    )
    {
        var activeDirection = GetActiveDirection(sortField);
        var targetDirection =
            activeDirection == DataGridSortDirection.Ascending
                ? DataGridSortDirection.Descending
                : DataGridSortDirection.Ascending;

        var linkOutput = new TagHelperOutput(
            "a",
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        linkOutput.Content.AppendHtml(headerContent);
        linkOutput.Content.AppendHtml(
            await RenderIcon(
                context,
                activeDirection switch
                {
                    DataGridSortDirection.Ascending => SemanticIconRole.SortAscending,
                    DataGridSortDirection.Descending => SemanticIconRole.SortDescending,
                    _ => SemanticIconRole.SortUnsorted,
                }
            )
        );

        var anchorTagHelper = new FrameworkAnchorTagHelper(_htmlGenerator)
        {
            ViewContext = ViewContext,
            Action = Action,
            Area = Area,
            Controller = Controller,
            Fragment = Fragment,
            Host = Host,
            Page = Page,
            PageHandler = PageHandler,
            Protocol = Protocol,
            Route = Route,
            RouteValues = SubstitutePlaceholders(sortField, targetDirection),
        };
        await anchorTagHelper.ProcessAsync(context, linkOutput);

        linkOutput.Attributes.SetAttribute("data-slot", "data-grid-sort-link");
        linkOutput.Attributes.SetAttribute(
            "data-active",
            activeDirection is null ? "false" : "true"
        );
        linkOutput.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-button", "sa-button-variant-ghost", "sa-button-size-sm")
        );
        // Align the button label with the unsorted columns' header text (sa-table-head pads
        // px-2, the small button px-2.5).
        linkOutput.Attributes.SetAttribute("style", "margin-left: -0.625rem");

        return linkOutput;
    }

    private IDictionary<string, string?> SubstitutePlaceholders(
        string sortField,
        DataGridSortDirection direction
    )
    {
        return RouteValues.ToDictionary(
            value => value.Key,
            value =>
                value
                    .Value?.Replace(SortFieldPlaceholder, sortField)
                    .Replace(SortDirectionPlaceholder, direction.GetQueryValueText()),
            StringComparer.OrdinalIgnoreCase
        );
    }

    private async Task<IHtmlContent> RenderIcon(TagHelperContext context, SemanticIconRole role)
    {
        var iconOutput = new TagHelperOutput(
            "svg",
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var iconTagHelper = new IconTagHelper(_iconOptions)
        {
            Name = _iconOptions.GetSemanticIconName(role),
        };
        await iconTagHelper.ProcessAsync(context, iconOutput);

        return iconOutput;
    }
}
