using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders page number, previous and next links for a paged list.
/// </summary>
/// <remarks>
///     <para>
///         Set the link destination with the routing attributes, such as <c>asp-page</c> and
///         <c>asp-route-*</c>. One route value must contain the <c>{page}</c> placeholder, such
///         as <c>asp-route-pageNo="{page}"</c>, which each link replaces with its page number. A
///         route value can also contain the <c>{pageSize}</c> placeholder.
///     </para>
///     <para>
///         Inside an <c>sa-data-grid</c>, the pager renders compact links in the grid's footer.
///         With one page, the footer still shows the item count and page sizes when they are
///         set. On its own, the pager renders labelled links, and nothing when there is one page
///         or less.
///     </para>
/// </remarks>
[HtmlTargetElement("sa-data-grid-pager")]
public class DataGridPagerTagHelper : StellarAdminAnchorTagHelperBase
{
    private const string PagePlaceholder = "{page}";
    private const string PageSizePlaceholder = "{pageSize}";

    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataGridPagerTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders the links.</param>
    /// <param name="iconOptions">
    ///     The icon options that supply the previous, next and ellipsis icons.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public DataGridPagerTagHelper(IHtmlGenerator htmlGenerator, IOptions<IconOptions> iconOptions)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Gets or sets the current page number.
    /// </summary>
    /// <value>
    ///     The page number, starting at 1.
    /// </value>
    /// <remarks>
    ///     Required.
    /// </remarks>
    [HtmlAttributeName("page")]
    public long? CurrentPage { get; set; }

    /// <summary>
    ///     Gets or sets the total number of pages.
    /// </summary>
    /// <remarks>
    ///     Required. The page links only render when there is more than one page.
    /// </remarks>
    [HtmlAttributeName("total-pages")]
    public long? TotalPages { get; set; }

    /// <summary>
    ///     Gets or sets the current number of items per page.
    /// </summary>
    /// <remarks>
    ///     With <c>total-items</c>, the grid's footer shows the range of items on the page, such as
    ///     "1–8 of 56". The page size selector marks this size as active. Required when a route
    ///     value contains <c>{pageSize}</c>.
    /// </remarks>
    [HtmlAttributeName("page-size")]
    public long? PageSize { get; set; }

    /// <summary>
    ///     Gets or sets the page sizes that the grid's footer offers.
    /// </summary>
    /// <value>
    ///     A comma-separated list of numbers, such as <c>8,24,48</c>.
    /// </value>
    /// <remarks>
    ///     The footer shows a link for each size under a "Rows per page" label. Each link goes to
    ///     page 1 with its size in the <c>{pageSize}</c> placeholder, so a route value must
    ///     contain it. Only shown inside an <c>sa-data-grid</c>.
    /// </remarks>
    [HtmlAttributeName("page-size-options")]
    public string? PageSizeOptions { get; set; }

    /// <summary>
    ///     Gets or sets the page size that the user chose.
    /// </summary>
    /// <remarks>
    ///     The page links put it in the <c>{pageSize}</c> placeholder, so the chosen size carries
    ///     across pages. Without it, the page links leave out the route value with the
    ///     placeholder. The links of the page size selector always carry their own size.
    /// </remarks>
    [HtmlAttributeName("selected-page-size")]
    public long? SelectedPageSize { get; set; }

    /// <summary>
    ///     Gets or sets the total number of items across all pages.
    /// </summary>
    /// <remarks>
    ///     The grid's footer shows the range of items on the page, such as "1–8 of 56", or the
    ///     number of items, such as "23 records", when there is one page.
    /// </remarks>
    [HtmlAttributeName("total-items")]
    public long? TotalItems { get; set; }

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

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     <see cref="CurrentPage" /> or <see cref="TotalPages" /> isn't set, no route value
    ///     contains <c>{page}</c>, a route value contains <c>{pageSize}</c> without
    ///     <see cref="PageSize" />, or <see cref="PageSizeOptions" /> isn't a list of numbers or
    ///     is set without a <c>{pageSize}</c> route value.
    /// </exception>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var gridContext = GetContext<DataGridContext>(context);
        if (gridContext is { Collecting: false })
        {
            // The data grid re-executes its child content once per row; the pager only acts
            // during the collect pass.
            output.SuppressOutput();
            return;
        }

        if (CurrentPage is not { } pageNo)
        {
            throw new InvalidOperationException(
                "<sa-data-grid-pager> requires the page attribute."
            );
        }

        if (TotalPages is not { } totalPages)
        {
            throw new InvalidOperationException(
                "<sa-data-grid-pager> requires the total-pages attribute."
            );
        }

        if (PreserveQuery ?? false)
        {
            foreach (var parameter in ViewContext.HttpContext.Request.Query)
            {
                if (!RouteValues.ContainsKey(parameter.Key))
                {
                    RouteValues[parameter.Key] = parameter.Value.ToString();
                }
            }
        }

        if (!RouteValues.Any(value => value.Value?.Contains(PagePlaceholder) == true))
        {
            throw new InvalidOperationException(
                "<sa-data-grid-pager> requires a route value containing the {page} page "
                    + "number placeholder, e.g. asp-route-pageNo=\"{page}\"."
            );
        }

        var hasPageSizePlaceholder = RouteValues.Any(value =>
            value.Value?.Contains(PageSizePlaceholder) == true
        );
        if (hasPageSizePlaceholder && PageSize is null)
        {
            throw new InvalidOperationException(
                "A {pageSize} placeholder on <sa-data-grid-pager> requires the page-size "
                    + "attribute, so the pager knows the size currently in effect."
            );
        }

        var pageSizeOptions = ParsePageSizeOptions();
        if (pageSizeOptions.Length > 0 && !hasPageSizePlaceholder)
        {
            throw new InvalidOperationException(
                "page-size-options on <sa-data-grid-pager> requires a route value containing "
                    + "the {pageSize} placeholder, e.g. asp-route-pageSize=\"{pageSize}\"."
            );
        }

        if (totalPages <= 1 && gridContext is null)
        {
            output.SuppressOutput();
            return;
        }

        // Within a grid the pager renders compact icon links; standalone keeps the
        // labelled Previous/Next links.
        var compact = gridContext is not null;

        TagBuilder? contentBuilder = null;
        if (totalPages > 1)
        {
            contentBuilder = new TagBuilder("ul");
            contentBuilder.Attributes.Add("data-slot", "pagination-content");
            contentBuilder.Attributes.Add("class", "sa-pagination-content");

            contentBuilder.InnerHtml.AppendHtml(
                WrapInItem(
                    await RenderNavigationLink(context, pageNo - 1, pageNo > 1, "previous", compact)
                )
            );

            long previousRenderedPage = 0;
            foreach (var page in GetPageWindow(pageNo, totalPages))
            {
                if (page - previousRenderedPage > 1)
                {
                    contentBuilder.InnerHtml.AppendHtml(WrapInItem(await RenderEllipsis(context)));
                }

                contentBuilder.InnerHtml.AppendHtml(
                    WrapInItem(await RenderPageLink(context, page, page == pageNo, compact))
                );
                previousRenderedPage = page;
            }

            contentBuilder.InnerHtml.AppendHtml(
                WrapInItem(
                    await RenderNavigationLink(
                        context,
                        pageNo + 1,
                        pageNo < totalPages,
                        "next",
                        compact
                    )
                )
            );
        }

        if (gridContext is not null)
        {
            var footerContent = new HtmlContentBuilder();
            if (pageSizeOptions.Length > 0)
            {
                footerContent.AppendHtml(await RenderPageSizeSelector(context, pageSizeOptions));
            }

            if (BuildRange(pageNo, totalPages) is { } range)
            {
                footerContent.AppendHtml(range);
            }

            if (contentBuilder is not null)
            {
                var navBuilder = new TagBuilder("nav");
                AddNavAttributes(navBuilder.Attributes, output.GetUserSuppliedClass());
                navBuilder.InnerHtml.AppendHtml(contentBuilder);
                footerContent.AppendHtml(navBuilder);
            }

            using var writer = new StringWriter();
            footerContent.WriteTo(writer, HtmlEncoder.Default);
            gridContext.PagerContent = writer.ToString() is { Length: > 0 } content
                ? content
                : null;

            output.SuppressOutput();
            return;
        }

        output.TagName = "nav";
        output.TagMode = TagMode.StartTagAndEndTag;
        var attributes = new Dictionary<string, string?>();
        AddNavAttributes(attributes, output.GetUserSuppliedClass());
        foreach (var attribute in attributes)
        {
            output.Attributes.SetAttribute(attribute.Key, attribute.Value);
        }

        output.Content.AppendHtml(contentBuilder);
    }

    /// <summary>
    ///     The record-range summary shown in the grid's footer: "1–8 of 56" when paging with
    ///     a known page size, otherwise the plain record count (e.g. "23 records"). Null when
    ///     <see cref="TotalItems" /> was not supplied.
    /// </summary>
    private TagBuilder? BuildRange(long pageNo, long totalPages)
    {
        if (TotalItems is not { } totalItems)
        {
            return null;
        }

        var valueBuilder = new TagBuilder("span");
        valueBuilder.Attributes.Add("class", "sa-data-grid-range-value");

        var rangeBuilder = new TagBuilder("span");
        rangeBuilder.Attributes.Add("data-slot", "data-grid-range");
        rangeBuilder.Attributes.Add("class", "sa-data-grid-range");

        if (totalPages > 1 && PageSize is { } pageSize)
        {
            var first = (pageNo - 1) * pageSize + 1;
            var last = Math.Min(pageNo * pageSize, totalItems);
            valueBuilder.InnerHtml.Append(
                $"{first.ToString(CultureInfo.InvariantCulture)}–{last.ToString(CultureInfo.InvariantCulture)}"
            );
            rangeBuilder.InnerHtml.AppendHtml(valueBuilder);
            rangeBuilder.InnerHtml.Append(
                $" of {totalItems.ToString(CultureInfo.InvariantCulture)}"
            );
        }
        else
        {
            valueBuilder.InnerHtml.Append(totalItems.ToString(CultureInfo.InvariantCulture));
            rangeBuilder.InnerHtml.AppendHtml(valueBuilder);
            rangeBuilder.InnerHtml.Append(totalItems == 1 ? " record" : " records");
        }

        return rangeBuilder;
    }

    /// <summary>
    ///     The footer's page-size selector: a labelled tab list with one link per size, the
    ///     current <see cref="PageSize" /> rendered as the active tab. Rides the library tabs
    ///     chassis (<c>sa-tabs</c>/<c>sa-tabs-list</c>, default variant) so themes style it
    ///     like any other tab list.
    /// </summary>
    private async Task<IHtmlContent> RenderPageSizeSelector(
        TagHelperContext context,
        long[] pageSizeOptions
    )
    {
        var labelId = $"{GetUniqueId(context)}-page-size-label";

        var labelBuilder = new TagBuilder("span");
        labelBuilder.Attributes.Add("id", labelId);
        labelBuilder.Attributes.Add("data-slot", "data-grid-page-size-label");
        labelBuilder.Attributes.Add("class", "sa-data-grid-page-size-label");
        labelBuilder.InnerHtml.Append("Rows per page");

        var tabListBuilder = new TagBuilder("div");
        tabListBuilder.Attributes.Add("data-slot", "tabs-list");
        tabListBuilder.Attributes.Add("data-variant", "default");
        tabListBuilder.Attributes.Add("aria-labelledby", labelId);
        tabListBuilder.Attributes.Add(
            "class",
            JoinCssClasses("sa-tabs-list", "sa-tabs-list-variant-default")
        );
        foreach (var pageSizeOption in pageSizeOptions)
        {
            tabListBuilder.InnerHtml.AppendHtml(await RenderPageSizeLink(context, pageSizeOption));
        }

        var tabsBuilder = new TagBuilder("div");
        tabsBuilder.Attributes.Add("data-slot", "tabs");
        tabsBuilder.Attributes.Add("data-orientation", "horizontal");
        tabsBuilder.Attributes.Add("class", JoinCssClasses("sa-tabs"));
        tabsBuilder.InnerHtml.AppendHtml(tabListBuilder);

        var selectorBuilder = new TagBuilder("div");
        selectorBuilder.Attributes.Add("data-slot", "data-grid-page-size");
        selectorBuilder.Attributes.Add("class", "sa-data-grid-page-size");
        selectorBuilder.InnerHtml.AppendHtml(labelBuilder);
        selectorBuilder.InnerHtml.AppendHtml(tabsBuilder);

        return selectorBuilder;
    }

    /// <summary>
    ///     Renders one page-size option by delegating to the library tab-link tag helper, so
    ///     the markup matches a hand-written <c>&lt;sa-tab-link&gt;</c>. The link targets page
    ///     1 with the option's page size.
    /// </summary>
    private async Task<IHtmlContent> RenderPageSizeLink(TagHelperContext context, long pageSize)
    {
        var linkOutput = new TagHelperOutput(
            "a",
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        linkOutput.Content.Append(pageSize.ToString(CultureInfo.InvariantCulture));

        var tabLinkTagHelper = new TabLinkTagHelper(_htmlGenerator)
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
            RouteValues = SubstitutePlaceholders(pageNo: 1, pageSize),
            Active = PageSize == pageSize,
        };
        await tabLinkTagHelper.ProcessAsync(context, linkOutput);

        return linkOutput;
    }

    private static void AddNavAttributes(IDictionary<string, string?> attributes, string userClass)
    {
        attributes["role"] = "navigation";
        attributes["aria-label"] = "pagination";
        attributes["data-slot"] = "data-grid-pager";
        attributes["class"] = JoinCssClasses("sa-pagination", userClass);
    }

    /// <summary>
    ///     The pages to show: the first and last page plus a window around the current page,
    ///     in ascending order. Gaps between them render as ellipses.
    /// </summary>
    private static IEnumerable<long> GetPageWindow(long pageNo, long totalPages)
    {
        var pages = new SortedSet<long> { 1, totalPages };
        for (var page = pageNo - 1; page <= pageNo + 1; page++)
        {
            if (page >= 1 && page <= totalPages)
            {
                pages.Add(page);
            }
        }

        return pages;
    }

    private static IHtmlContent WrapInItem(IHtmlContent content)
    {
        var itemBuilder = new TagBuilder("li");
        itemBuilder.Attributes.Add("data-slot", "pagination-item");
        itemBuilder.InnerHtml.AppendHtml(content);

        return itemBuilder;
    }

    private long[] ParsePageSizeOptions()
    {
        if (PageSizeOptions is not { } optionsText)
        {
            return [];
        }

        try
        {
            return optionsText
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(option => long.Parse(option, CultureInfo.InvariantCulture))
                .ToArray();
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(
                "page-size-options on <sa-data-grid-pager> must be a comma-separated list of "
                    + $"numbers (e.g. \"8,24,48\"), but was \"{optionsText}\".",
                exception
            );
        }
    }

    /// <summary>
    ///     Builds the route values of one link by substituting the page-number and page-size
    ///     placeholders. Page links pass the selected page size — dropping the placeholder's
    ///     route value when none was chosen, so URLs only carry a size the user picked;
    ///     page-size links pass their target size and page 1.
    /// </summary>
    private IDictionary<string, string?> SubstitutePlaceholders(long pageNo, long? pageSize)
    {
        var pageNoText = pageNo.ToString(CultureInfo.InvariantCulture);
        var pageSizeText = pageSize?.ToString(CultureInfo.InvariantCulture);

        var routeValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in RouteValues)
        {
            if (pageSizeText is null && value?.Contains(PageSizePlaceholder) == true)
            {
                continue;
            }

            var substituted = value?.Replace(PagePlaceholder, pageNoText);
            if (pageSizeText is not null)
            {
                substituted = substituted?.Replace(PageSizePlaceholder, pageSizeText);
            }

            routeValues[key] = substituted;
        }

        return routeValues;
    }

    private PaginationLinkTagHelper CreatePageLinkTagHelper(
        long targetPage,
        bool isActive,
        bool compact
    )
    {
        return new PaginationLinkTagHelper(_htmlGenerator)
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
            RouteValues = SubstitutePlaceholders(targetPage, SelectedPageSize),
            Active = isActive,
            Size = compact ? ButtonSize.IconSmall : null,
        };
    }

    private async Task<IHtmlContent> RenderPageLink(
        TagHelperContext context,
        long page,
        bool isActive,
        bool compact
    )
    {
        var linkOutput = new TagHelperOutput(
            "a",
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        linkOutput.Content.Append(page.ToString(CultureInfo.InvariantCulture));

        await CreatePageLinkTagHelper(page, isActive, compact).ProcessAsync(context, linkOutput);

        return linkOutput;
    }

    private async Task<IHtmlContent> RenderNavigationLink(
        TagHelperContext context,
        long targetPage,
        bool isEnabled,
        string direction,
        bool compact
    )
    {
        // Compact (in-grid) links are icon-only, so they carry an aria-label instead of
        // the visible label span.
        var content = new DefaultTagHelperContent();
        if (direction == "previous")
        {
            content.AppendHtml(await RenderIcon(context, SemanticIconRole.PaginationPrevious));
            if (!compact)
            {
                content.AppendHtml("<span class=\"sa-pagination-link-label\">Previous</span>");
            }
        }
        else
        {
            if (!compact)
            {
                content.AppendHtml("<span class=\"sa-pagination-link-label\">Next</span>");
            }
            content.AppendHtml(await RenderIcon(context, SemanticIconRole.PaginationNext));
        }

        var ariaLabel = direction == "previous" ? "Go to previous page" : "Go to next page";

        if (!isEnabled)
        {
            var disabledBuilder = new TagBuilder("a");
            disabledBuilder.Attributes.Add("aria-disabled", "true");
            disabledBuilder.Attributes.Add("data-slot", "pagination-link");
            disabledBuilder.Attributes.Add("data-active", "false");
            if (compact)
            {
                disabledBuilder.Attributes.Add("aria-label", ariaLabel);
            }
            disabledBuilder.Attributes.Add(
                "class",
                JoinCssClasses(
                    "sa-button",
                    "sa-button-variant-ghost",
                    compact ? "sa-button-size-icon-sm" : "sa-button-size-default",
                    "sa-pagination-link",
                    $"sa-pagination-{direction}"
                )
            );
            disabledBuilder.Attributes.Add("style", "pointer-events: none; opacity: 0.5");
            disabledBuilder.InnerHtml.AppendHtml(content);

            return disabledBuilder;
        }

        var linkAttributes = new List<TagHelperAttribute>
        {
            new TagHelperAttribute("class", $"sa-pagination-{direction}"),
        };
        if (compact)
        {
            linkAttributes.Add(new TagHelperAttribute("aria-label", ariaLabel));
        }

        var linkOutput = new TagHelperOutput(
            "a",
            new TagHelperAttributeList(linkAttributes),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        linkOutput.Content.AppendHtml(content);

        await CreatePageLinkTagHelper(targetPage, isActive: false, compact)
            .ProcessAsync(context, linkOutput);

        return linkOutput;
    }

    private async Task<IHtmlContent> RenderEllipsis(TagHelperContext context)
    {
        var ellipsisOutput = new TagHelperOutput(
            "span",
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var ellipsisTagHelper = new PaginationEllipsisTagHelper(_iconOptions);
        await ellipsisTagHelper.ProcessAsync(context, ellipsisOutput);

        return ellipsisOutput;
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
