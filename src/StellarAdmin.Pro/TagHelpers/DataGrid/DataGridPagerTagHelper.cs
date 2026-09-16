using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers;
using StellarAdmin.TagHelpers.Icons;

namespace StellarAdmin.Pro.TagHelpers;

/// <summary>
///     A pager that renders page-number, previous, and next links for a paged data set. Link
///     destinations are specified with the standard routing attributes (<c>asp-page</c>,
///     <c>asp-action</c>, <c>asp-route-*</c>, etc.); a route value must contain the
///     <c>{pageNo}</c> placeholder, which is replaced with the target page number in each
///     link (e.g. <c>asp-route-pageNo="{pageNo}"</c>). A route value may also contain the
///     <c>{pageSize}</c> placeholder: page links substitute <c>selected-page-size</c> when
///     one is supplied and drop the route value otherwise, so a size carries across page
///     navigation only when one was actually chosen, while the links of the footer's
///     page-size selector (<c>page-size-options</c>) always substitute the target size and
///     reset the page number to 1. Declared as a child of <c>sa-data-grid</c> it renders in the grid's
///     footer bar as compact icon links, alongside a record-range summary when
///     <c>total-items</c> is supplied — the footer renders even when there is a single page,
///     showing just the record count. Used standalone it renders the classic labelled pager
///     and nothing at all when there is one page or less.
/// </summary>
[HtmlTargetElement("sa-data-grid-pager")]
public class DataGridPagerTagHelper : StellarAdminAnchorTagHelperBase
{
    private const string PageNoPlaceholder = "{pageNo}";
    private const string PageSizePlaceholder = "{pageSize}";

    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IIconManager _iconManager;

    public DataGridPagerTagHelper(IHtmlGenerator htmlGenerator, IIconManager iconManager)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconManager = iconManager ?? throw new ArgumentNullException(nameof(iconManager));
    }

    /// <summary>
    ///     The current page number (1-based).
    /// </summary>
    [HtmlAttributeName("page-no")]
    public long? PageNo { get; set; }

    /// <summary>
    ///     The total number of pages. The page links render only when there is more than one
    ///     page; within a grid the footer itself still renders with the record-range summary.
    /// </summary>
    [HtmlAttributeName("total-pages")]
    public long? TotalPages { get; set; }

    /// <summary>
    ///     The current number of items per page. Used together with <see cref="TotalItems" />
    ///     to render the record-range summary (e.g. "1–8 of 56") in the grid's footer, and
    ///     used to mark the active option of the page-size selector.
    /// </summary>
    [HtmlAttributeName("page-size")]
    public long? PageSize { get; set; }

    /// <summary>
    ///     The page sizes offered by the grid footer's page-size selector, comma-separated
    ///     (e.g. "8,24,48"). When supplied, the footer renders a "Rows per page" tab list of
    ///     links, one per size; the current <see cref="PageSize" /> renders as the active tab.
    ///     Each link substitutes its size for the <c>{pageSize}</c> placeholder and resets the
    ///     <c>{pageNo}</c> placeholder to 1. Only rendered within an <c>sa-data-grid</c>.
    /// </summary>
    [HtmlAttributeName("page-size-options")]
    public string? PageSizeOptions { get; set; }

    /// <summary>
    ///     The page size the user explicitly chose, substituted for the <c>{pageSize}</c>
    ///     placeholder in page links so a chosen size carries across navigation. When
    ///     omitted, page links drop the placeholder's route value entirely, keeping URLs
    ///     free of a size the user never picked. The page-size selector's links are
    ///     unaffected — each always carries its target size.
    /// </summary>
    [HtmlAttributeName("selected-page-size")]
    public long? SelectedPageSize { get; set; }

    /// <summary>
    ///     The total number of items across all pages. When supplied, the grid's footer shows
    ///     a record-range summary: "1–8 of 56" when paging, or the plain record count
    ///     (e.g. "23 records") when everything fits on one page.
    /// </summary>
    [HtmlAttributeName("total-items")]
    public long? TotalItems { get; set; }

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

        if (PageNo is not { } pageNo)
        {
            throw new InvalidOperationException(
                "<sa-data-grid-pager> requires the page-no attribute."
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

        if (!RouteValues.Any(value => value.Value?.Contains(PageNoPlaceholder) == true))
        {
            throw new InvalidOperationException(
                "<sa-data-grid-pager> requires a route value containing the {pageNo} page "
                    + "number placeholder, e.g. asp-route-pageNo=\"{pageNo}\"."
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
            JoinCssClasses("sa-tabs-list", "group/tabs-list", "sa-tabs-list-variant-default")
        );
        foreach (var pageSizeOption in pageSizeOptions)
        {
            tabListBuilder.InnerHtml.AppendHtml(await RenderPageSizeLink(context, pageSizeOption));
        }

        var tabsBuilder = new TagBuilder("div");
        tabsBuilder.Attributes.Add("data-slot", "tabs");
        tabsBuilder.Attributes.Add("data-orientation", "horizontal");
        tabsBuilder.Attributes.Add("class", JoinCssClasses("sa-tabs", "group/tabs"));
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
            IsActive = PageSize == pageSize,
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

            var substituted = value?.Replace(PageNoPlaceholder, pageNoText);
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
            IsActive = isActive,
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
            content.AppendHtml(await RenderIcon(context, "chevron-left"));
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
            content.AppendHtml(await RenderIcon(context, "chevron-right"));
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
                    "group/button",
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
        var ellipsisTagHelper = new PaginationEllipsisTagHelper(_iconManager);
        await ellipsisTagHelper.ProcessAsync(context, ellipsisOutput);

        return ellipsisOutput;
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
