using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.Icons;
using FrameworkAnchorTagHelper = Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Declares how the data grid generates its column-sorting links, and the current sort
///     state. Link destinations are specified with the standard routing attributes
///     (<c>asp-page</c>, <c>asp-action</c>, <c>asp-route-*</c>, etc.); the route values must
///     contain the <c>{sort}</c> placeholder, replaced with the column's sort field, and the
///     <c>{dir}</c> placeholder, replaced with the target direction (<c>asc</c> or
///     <c>desc</c>) — e.g. <c>asp-route-sortBy="{sort}" asp-route-sortDir="{dir}"</c>.
///     Renders nothing itself; columns opt in with their <c>sortable</c> attribute.
/// </summary>
[HtmlTargetElement(
    "sa-data-grid-sort",
    ParentTag = "sa-data-grid",
    TagStructure = TagStructure.WithoutEndTag
)]
public class DataGridSortTagHelper : StellarAdminAnchorTagHelperBase
{
    private const string SortFieldPlaceholder = "{sort}";
    private const string DirectionPlaceholder = "{dir}";

    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IIconManager _iconManager;

    public DataGridSortTagHelper(IHtmlGenerator htmlGenerator, IIconManager iconManager)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconManager = iconManager ?? throw new ArgumentNullException(nameof(iconManager));
    }

    /// <summary>
    ///     The sort field the data is currently sorted by, matched (case-insensitively)
    ///     against the columns' sort fields to mark the active column. Omit when the data is
    ///     unsorted.
    /// </summary>
    [HtmlAttributeName("sort-by")]
    public string? SortBy { get; set; }

    /// <summary>
    ///     The direction the data is currently sorted in.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="DataGridSortDirection.Ascending" />.
    /// </remarks>
    [HtmlAttributeName("sort-direction")]
    public DataGridSortDirection? SortDirection { get; set; }

    /// <summary>
    ///     Whether to carry the current request's query string parameters over into the
    ///     generated links. Only parameters not already declared as route values are added, so
    ///     explicit <c>asp-route-*</c> values always win, regardless of attribute order.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    [HtmlAttributeName("preserve-query")]
    public bool? PreserveQuery { get; set; }

    /// <summary>
    ///     Query string parameters, comma-separated, that <c>preserve-query</c> must not
    ///     carry over into the generated links (e.g. <c>preserve-query-except="pageNo"</c>).
    ///     Use it for parameters a sort click resets: leaving the parameter out of the link
    ///     falls back to its default, without pinning that default into the URL.
    /// </summary>
    [HtmlAttributeName("preserve-query-except")]
    public string? PreserveQueryExcept { get; set; }

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
                "<sa-data-grid-sort> requires a route value containing the {sort} sort field "
                    + "placeholder, e.g. asp-route-sortBy=\"{sort}\"."
            );
        }

        if (!RouteValues.Any(value => value.Value?.Contains(DirectionPlaceholder) == true))
        {
            throw new InvalidOperationException(
                "<sa-data-grid-sort> requires a route value containing the {dir} sort "
                    + "direction placeholder, e.g. asp-route-sortDir=\"{dir}\"."
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
        return string.Equals(sortField, SortBy, StringComparison.OrdinalIgnoreCase)
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
                    DataGridSortDirection.Ascending => "arrow-up",
                    DataGridSortDirection.Descending => "arrow-down",
                    _ => "chevrons-up-down",
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
            JoinCssClasses(
                "sa-button",
                "group/button",
                "sa-button-variant-ghost",
                "sa-button-size-sm"
            )
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
                    .Replace(DirectionPlaceholder, direction.GetQueryValueText()),
            StringComparer.OrdinalIgnoreCase
        );
    }

    private async Task<IHtmlContent> RenderIcon(TagHelperContext context, string name)
    {
        var iconOutput = new TagHelperOutput(
            "svg",
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var iconTagHelper = new IconTagHelper(_iconManager) { Name = name };
        await iconTagHelper.ProcessAsync(context, iconOutput);

        return iconOutput;
    }
}
