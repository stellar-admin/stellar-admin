using System.Collections;
using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A data grid that renders a full table from declarative column definitions and a data
///     source. Columns are declared with <c>sa-data-grid-column</c>; the grid renders the
///     header from the column definitions and re-renders the column content once per data row.
///     The grid renders as a card-style panel; add an <c>sa-data-grid-pager</c> child to
///     render a footer bar with paging links and a record-range summary, an
///     <c>sa-data-grid-sort</c> child to render sort links on the sortable columns' headers,
///     and an <c>sa-data-grid-selection</c> child to render a row-selection checkbox column.
///     Querying, sorting, and paging the data remain the application's responsibility — the
///     grid renders the items it is given.
/// </summary>
[HtmlTargetElement("sa-data-grid")]
public class DataGridTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IIconManager _iconManager;

    public DataGridTagHelper(IHtmlGenerator htmlGenerator, IIconManager iconManager)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconManager = iconManager ?? throw new ArgumentNullException(nameof(iconManager));
    }

    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <summary>
    ///     The data items to render, one table row per item. When paging, pass the current
    ///     page of items, not the full data set.
    /// </summary>
    [HtmlAttributeName("items")]
    public IEnumerable? Items { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Ensures we work with a list which has better performance that an enumerable
        var items = Items?.Cast<object?>().ToList() ?? [];

        // Set contexts
        var gridContext = new DataGridContext
        {
            ItemType = items.Find(item => item is not null)?.GetType(),
        };
        SetContext(context, gridContext);
        var idDiscriminator = new UniqueIdDiscriminator();
        SetContext(context, idDiscriminator);

        // Collect pass (CurrentRow is null): columns register their definitions and produce
        // no output.
        await output.GetChildContentAsync(useCachedResult: false);

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("data-slot", "data-grid");
        // The grid rides the card chassis: sa-card supplies the themed panel (surface,
        // ring/shadow, --card-spacing) and sa-data-grid neutralizes the card's padding so
        // the table bleeds to the panel edges.
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-data-grid", "sa-card", "group/card", output.GetUserSuppliedClass())
        );

        var tableBuilder = new TagBuilder("table");
        tableBuilder.Attributes.Add("data-slot", "table");
        tableBuilder.Attributes.Add("class", "sa-table");

        // Render table header
        tableBuilder.InnerHtml.AppendHtml(await RenderHeader(context, gridContext));

        // Render table body (rows)
        tableBuilder.InnerHtml.AppendHtml(
            await RenderBody(context, output, gridContext, items, idDiscriminator)
        );

        // Place table inside a container
        var containerBuilder = new TagBuilder("div");
        containerBuilder.Attributes.Add("data-slot", "table-container");
        containerBuilder.Attributes.Add("class", "sa-table-container");
        containerBuilder.InnerHtml.AppendHtml(tableBuilder);

        // Optionally places table inside table selection web component which
        // assists with managing table selection state
        if (gridContext.Selection is { } selection)
        {
            var selectionBuilder = new TagBuilder("sel-table-selection");
            selectionBuilder.Attributes.Add("data-slot", "table-selection");
            if (selection.Id is not null)
            {
                selectionBuilder.Attributes.Add("id", selection.Id);
            }
            if (selection.CssClass is not null)
            {
                selectionBuilder.Attributes.Add("class", selection.CssClass);
            }
            selectionBuilder.InnerHtml.AppendHtml(containerBuilder);
            output.Content.AppendHtml(selectionBuilder);
        }
        else
        {
            output.Content.AppendHtml(containerBuilder);
        }

        // If we have a pager, add a footer with the rendered pager
        if (gridContext.PagerContent is not null)
        {
            // The card-footer slot and class make the footer inherit the theme's card
            // footer treatment (background, horizontal padding, bottom rounding).
            var footerBuilder = new TagBuilder("div");
            footerBuilder.Attributes.Add("data-slot", "card-footer");
            footerBuilder.Attributes.Add(
                "class",
                JoinCssClasses("sa-card-footer", "sa-data-grid-footer")
            );
            footerBuilder.InnerHtml.AppendHtml(gridContext.PagerContent);
            output.Content.AppendHtml(footerBuilder);
        }
    }

    private async Task<IHtmlContent> RenderHeader(
        TagHelperContext context,
        DataGridContext gridContext
    )
    {
        var headerRowBuilder = new TagBuilder("tr");
        headerRowBuilder.Attributes.Add("data-slot", "table-row");
        headerRowBuilder.Attributes.Add("class", "sa-table-row");

        if (gridContext.Selection is not null)
        {
            var selectAllHeadBuilder = new TagBuilder("th");
            selectAllHeadBuilder.Attributes.Add("data-slot", "table-head");
            selectAllHeadBuilder.Attributes.Add("class", "sa-table-head");
            selectAllHeadBuilder.Attributes.Add("style", "width: 3rem");
            selectAllHeadBuilder.InnerHtml.AppendHtml(
                await RenderCheckbox(
                    context,
                    [
                        new TagHelperAttribute("type", "checkbox"),
                        new TagHelperAttribute("aria-label", "Select all rows"),
                    ]
                )
            );
            headerRowBuilder.InnerHtml.AppendHtml(selectAllHeadBuilder);
        }

        foreach (var column in gridContext.Columns)
        {
            var headBuilder = new TagBuilder("th");
            headBuilder.Attributes.Add("data-slot", "table-head");
            headBuilder.Attributes.Add("class", JoinCssClasses("sa-table-head", column.CssClass));

            var headerContent = new HtmlContentBuilder();
            if (column.HeaderHtml is not null)
            {
                headerContent.AppendHtml(column.HeaderHtml);
            }
            else if (column.Title is not null)
            {
                headerContent.Append(column.Title);
            }

            if (column is { Sortable: true, SortField: { } sortField })
            {
                var sort =
                    gridContext.Sort
                    ?? throw new InvalidOperationException(
                        "A sortable <sa-data-grid-column> requires an <sa-data-grid-sort> "
                            + "child on the grid declaring the sort URL."
                    );

                if (sort.GetActiveDirection(sortField) is { } activeDirection)
                {
                    headBuilder.Attributes.Add(
                        "aria-sort",
                        activeDirection == DataGridSortDirection.Ascending
                            ? "ascending"
                            : "descending"
                    );
                }

                headBuilder.InnerHtml.AppendHtml(
                    await sort.RenderSortLink(context, sortField, headerContent)
                );
            }
            else
            {
                headBuilder.InnerHtml.AppendHtml(headerContent);
            }

            headerRowBuilder.InnerHtml.AppendHtml(headBuilder);
        }

        var headerBuilder = new TagBuilder("thead");
        headerBuilder.Attributes.Add("data-slot", "table-header");
        headerBuilder.Attributes.Add("class", "sa-table-header");
        headerBuilder.InnerHtml.AppendHtml(headerRowBuilder);

        return headerBuilder;
    }

    private async Task<IHtmlContent> RenderBody(
        TagHelperContext context,
        TagHelperOutput output,
        DataGridContext gridContext,
        List<object?> items,
        UniqueIdDiscriminator idDiscriminator
    )
    {
        var bodyBuilder = new TagBuilder("tbody");
        bodyBuilder.Attributes.Add("data-slot", "table-body");
        bodyBuilder.Attributes.Add("class", "sa-table-body");

        if (items.Count == 0)
        {
            var columnCount = gridContext.Columns.Count + (gridContext.Selection is null ? 0 : 1);

            var emptyCellBuilder = new TagBuilder("td");
            emptyCellBuilder.Attributes.Add("data-slot", "table-cell");
            emptyCellBuilder.Attributes.Add("class", "sa-table-cell");
            emptyCellBuilder.Attributes.Add(
                "colspan",
                Math.Max(columnCount, 1).ToString(CultureInfo.InvariantCulture)
            );
            emptyCellBuilder.Attributes.Add("style", "height: 6rem; text-align: center");

            if (gridContext.EmptyContent is not null)
            {
                emptyCellBuilder.InnerHtml.AppendHtml(gridContext.EmptyContent);
            }
            else
            {
                emptyCellBuilder.InnerHtml.Append("No records found.");
            }

            var emptyRowBuilder = new TagBuilder("tr");
            emptyRowBuilder.Attributes.Add("data-slot", "table-row");
            emptyRowBuilder.Attributes.Add("class", "sa-table-row");
            emptyRowBuilder.InnerHtml.AppendHtml(emptyCellBuilder);

            bodyBuilder.InnerHtml.AppendHtml(emptyRowBuilder);
            return bodyBuilder;
        }

        // Preserve any outer value so nested grids restore correctly.
        var priorItem = ViewContext.ViewData[DataGridConsts.ItemKey];
        try
        {
            for (var i = 0; i < items.Count; i++)
            {
                var currentRow = new DataGridRowContext { Item = items[i], RowIndex = i };
                gridContext.CurrentRow = currentRow;
                ViewContext.ViewData[DataGridConsts.ItemKey] = items[i];
                idDiscriminator.Value = $"r{i}";

                // Row pass: re-execute the grid's child content; columns deposit their cells.
                await output.GetChildContentAsync(useCachedResult: false);

                var rowBuilder = new TagBuilder("tr");
                rowBuilder.Attributes.Add("data-slot", "table-row");
                rowBuilder.Attributes.Add("class", "sa-table-row");

                if (gridContext.Selection is { } selection)
                {
                    var key =
                        (
                            items[i] is { } item
                                ? DataGridFieldGetters.GetValue(item, selection.KeyField)
                                : null
                        )?.ToString() ?? "";

                    var checkboxAttributes = new List<TagHelperAttribute>
                    {
                        new TagHelperAttribute("type", "checkbox"),
                        new TagHelperAttribute("value", key),
                        new TagHelperAttribute("aria-label", $"Select row {key}"),
                    };
                    if (selection.Name is not null)
                    {
                        checkboxAttributes.Add(new TagHelperAttribute("name", selection.Name));
                    }

                    var selectCellBuilder = new TagBuilder("td");
                    selectCellBuilder.Attributes.Add("data-slot", "table-cell");
                    selectCellBuilder.Attributes.Add("class", "sa-table-cell");
                    selectCellBuilder.InnerHtml.AppendHtml(
                        await RenderCheckbox(context, checkboxAttributes)
                    );
                    rowBuilder.InnerHtml.AppendHtml(selectCellBuilder);
                }

                foreach (var cell in currentRow.Cells)
                {
                    var cellBuilder = new TagBuilder("td");
                    cellBuilder.Attributes.Add("data-slot", "table-cell");
                    cellBuilder.Attributes.Add(
                        "class",
                        JoinCssClasses("sa-table-cell", cell.CssClass)
                    );
                    cellBuilder.InnerHtml.AppendHtml(cell.Html);
                    rowBuilder.InnerHtml.AppendHtml(cellBuilder);
                }

                bodyBuilder.InnerHtml.AppendHtml(rowBuilder);
            }
        }
        finally
        {
            if (priorItem is null)
            {
                ViewContext.ViewData.Remove(DataGridConsts.ItemKey);
            }
            else
            {
                ViewContext.ViewData[DataGridConsts.ItemKey] = priorItem;
            }

            gridContext.CurrentRow = null;
            idDiscriminator.Value = null;
        }

        return bodyBuilder;
    }

    /// <summary>
    ///     Renders a selection checkbox by delegating to the library input tag helper, so the
    ///     markup (wrapper, input, indicator) matches a hand-written
    ///     <c>&lt;sa-input type="checkbox"&gt;</c>. The type/name/value are passed as element
    ///     attributes rather than bound properties — the helper copies bound properties from
    ///     the host element's attribute list, which this synthetic output does not have.
    /// </summary>
    private async Task<IHtmlContent> RenderCheckbox(
        TagHelperContext context,
        List<TagHelperAttribute> attributes
    )
    {
        var checkboxOutput = new TagHelperOutput(
            "sa-input",
            new TagHelperAttributeList(attributes),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var inputTagHelper = new InputTagHelper(_htmlGenerator, _iconManager)
        {
            ViewContext = ViewContext,
        };
        await inputTagHelper.ProcessAsync(context, checkboxOutput);

        return checkboxOutput;
    }
}
