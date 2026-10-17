using System.Collections;
using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     A data grid that renders a full table from declarative column definitions and a data
///     source. Columns are declared with <c>sa-data-grid-column</c>; the grid renders the
///     header from the column definitions and re-renders the column content once per data row.
///     The grid renders as a card-style panel; add an <c>sa-data-grid-pager</c> child to
///     render a footer bar with paging links and a record-range summary, an
///     <c>sa-data-grid-sort</c> child to render sort links on the sortable columns' headers,
///     an <c>sa-data-grid-selection</c> child to render a row-selection checkbox column, and an
///     <c>sa-data-grid-add-row</c> child to render a row for a button that adds an item.
///     Rows listed in <c>editing-keys</c>, and a <c>new-item</c>, render in edit mode with their
///     columns' <c>sa-data-grid-edit-template</c>s. Querying, sorting, and paging the data remain the application's responsibility — the
///     grid renders the items it is given.
/// </summary>
[HtmlTargetElement("sa-data-grid")]
public class DataGridTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    public DataGridTagHelper(IHtmlGenerator htmlGenerator, IOptions<IconOptions> iconOptions)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconOptions = iconOptions.Value;
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

    /// <summary>
    ///     The name of a public property on the row item whose value identifies the row. Row
    ///     selection and expanded details use it, and a row keeps them when it is replaced in
    ///     the page.
    /// </summary>
    [HtmlAttributeName("key-field")]
    public string? KeyField { get; set; }

    /// <summary>
    ///     The keys of the rows to render in edit mode, with their columns' edit templates.
    ///     Requires <c>key-field</c>.
    /// </summary>
    [HtmlAttributeName("editing-keys")]
    public IEnumerable? EditingKeys { get; set; }

    /// <summary>
    ///     An item to render in edit mode above the other rows, with the key <c>new</c>. Its
    ///     columns without an edit template are empty. Requires <c>key-field</c>.
    /// </summary>
    [HtmlAttributeName("new-item")]
    public object? NewItem { get; set; }

    /// <summary>
    ///     Renders only the rows, without the grid, its table and its header, for a response
    ///     that replaces rows in the page. A row in edit mode renders without its details row.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    [HtmlAttributeName("rows-only")]
    public bool RowsOnly { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Razor writes a null expression bound to a string attribute as an empty string.
        KeyField = string.IsNullOrEmpty(KeyField) ? null : KeyField;

        if ((EditingKeys is not null || NewItem is not null) && KeyField is null)
        {
            throw new InvalidOperationException(
                "<sa-data-grid> requires the key-field attribute when editing-keys or new-item "
                    + "is set."
            );
        }

        // Ensures we work with a list which has better performance that an enumerable
        var items = Items?.Cast<object?>().ToList() ?? [];

        // Set contexts
        var gridContext = new DataGridContext
        {
            ItemType = (NewItem ?? items.Find(item => item is not null))?.GetType(),
            KeyField = KeyField,
        };
        SetContext(context, gridContext);
        var idDiscriminator = new UniqueIdDiscriminator();
        SetContext(context, idDiscriminator);

        // Collect pass (CurrentRow is null): columns register their definitions and produce
        // no output.
        await output.GetChildContentAsync(useCachedResult: false);

        if (RowsOnly)
        {
            output.TagName = null;
            output.Content.SetHtmlContent(
                await RenderRows(context, output, gridContext, items, idDiscriminator)
            );
            return;
        }

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("data-slot", "data-grid");
        // The grid rides the card chassis: sa-card supplies the themed panel (surface,
        // ring/shadow, --card-spacing) and sa-data-grid neutralizes the card's padding so
        // the table bleeds to the panel edges.
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-data-grid", "sa-card", output.GetUserSuppliedClass())
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

        // Places the table inside the table web component, which adds the row selection, row
        // details and editing behaviors
        var selection = gridContext.Selection;
        var rowDetail = gridContext.RowDetail;
        var editing = gridContext.Columns.Any(column => column.HasEditTemplate);
        if (selection is not null || rowDetail is not null || editing)
        {
            var tableElementBuilder = new TagBuilder("sel-table");
            foreach (
                var (name, value) in TableElementRendering.GetAttributes(
                    selection is not null,
                    rowDetail?.Settings,
                    editing
                )
            )
            {
                tableElementBuilder.Attributes[name] = value;
            }
            if (selection?.Id is not null)
            {
                tableElementBuilder.Attributes["id"] = selection.Id;
            }
            if (selection?.CssClass is not null)
            {
                tableElementBuilder.AddCssClass(selection.CssClass);
            }
            tableElementBuilder.InnerHtml.AppendHtml(containerBuilder);
            output.Content.AppendHtml(tableElementBuilder);
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

        if (gridContext.RowDetail is { Toggle: DataGridRowDetailToggle.Start })
        {
            headerRowBuilder.InnerHtml.AppendHtml(
                await RenderToggleCell(context, "th", "table-head", "sa-table-head", all: true)
            );
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

        if (gridContext.RowDetail is { Toggle: DataGridRowDetailToggle.End })
        {
            headerRowBuilder.InnerHtml.AppendHtml(
                await RenderToggleCell(context, "th", "table-head", "sa-table-head", all: true)
            );
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

        var addRow = gridContext.AddRow is { } addRowDefinition
            ? RenderAddRow(gridContext, addRowDefinition.Content)
            : null;
        if (gridContext.AddRow is { Position: DataGridAddRowPosition.Top })
        {
            bodyBuilder.InnerHtml.AppendHtml(addRow!);
        }

        if (items.Count == 0 && NewItem is null)
        {
            var columnCount = GetColumnCount(gridContext);

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

            // Marked, so the empty state can step aside for a row added in the page
            var emptyRowBuilder = new TagBuilder("tr");
            emptyRowBuilder.Attributes.Add("data-slot", "table-row");
            emptyRowBuilder.Attributes.Add("class", "sa-table-row");
            emptyRowBuilder.Attributes.Add("data-empty", "");
            emptyRowBuilder.InnerHtml.AppendHtml(emptyCellBuilder);

            bodyBuilder.InnerHtml.AppendHtml(emptyRowBuilder);
        }
        else
        {
            bodyBuilder.InnerHtml.AppendHtml(
                await RenderRows(context, output, gridContext, items, idDiscriminator)
            );
        }

        if (gridContext.AddRow is { Position: DataGridAddRowPosition.Bottom })
        {
            bodyBuilder.InnerHtml.AppendHtml(addRow!);
        }

        return bodyBuilder;
    }

    private static TagBuilder RenderAddRow(DataGridContext gridContext, string content)
    {
        var cellBuilder = new TagBuilder("td");
        cellBuilder.Attributes.Add("data-slot", "table-cell");
        cellBuilder.Attributes.Add("class", "sa-table-cell");
        cellBuilder.Attributes.Add(
            "colspan",
            Math.Max(GetColumnCount(gridContext), 1).ToString(CultureInfo.InvariantCulture)
        );
        cellBuilder.InnerHtml.AppendHtml(content);

        var rowBuilder = new TagBuilder("tr");
        rowBuilder.Attributes.Add("data-slot", "table-add-row");
        rowBuilder.Attributes.Add("class", "sa-table-row sa-table-add-row");
        rowBuilder.InnerHtml.AppendHtml(cellBuilder);

        return rowBuilder;
    }

    /// <summary>
    ///     Renders a row per item, after the new item's row, with each display row's details
    ///     row.
    /// </summary>
    private async Task<IHtmlContent> RenderRows(
        TagHelperContext context,
        TagHelperOutput output,
        DataGridContext gridContext,
        List<object?> items,
        UniqueIdDiscriminator idDiscriminator
    )
    {
        // The details rows' ids start with the grid's id, or the id of its source tag
        var gridId =
            output.Attributes.TryGetAttribute("id", out var idAttribute)
            && idAttribute.Value?.ToString() is { Length: > 0 } id
                ? id
                : context.UniqueId;

        var editingKeys = (EditingKeys?.Cast<object?>() ?? [])
            .Select(key => Convert.ToString(key, CultureInfo.InvariantCulture))
            .OfType<string>()
            .ToHashSet();
        var rows = new List<(object? Item, string? Key, bool Editing)>();
        if (NewItem is not null)
        {
            rows.Add((NewItem, "new", true));
        }
        foreach (var item in items)
        {
            var key = KeyField is null ? null : DataGridFieldGetters.GetKey(item, KeyField);
            rows.Add((item, key, key is not null && editingKeys.Contains(key)));
        }

        var rowsBuilder = new HtmlContentBuilder();

        // Preserve any outer value so nested grids restore correctly.
        var priorItem = ViewContext.ViewData[DataGridConsts.ItemKey];
        var templateInfo = ViewContext.ViewData.TemplateInfo;
        var priorPrefix = templateInfo.HtmlFieldPrefix;
        try
        {
            for (var i = 0; i < rows.Count; i++)
            {
                var (item, rowKey, editing) = rows[i];
                var currentRow = new DataGridRowContext
                {
                    Item = item,
                    RowIndex = i,
                    Editing = editing,
                    IsNew = NewItem is not null && i == 0,
                };
                gridContext.CurrentRow = currentRow;
                ViewContext.ViewData[DataGridConsts.ItemKey] = item;
                idDiscriminator.Value = rowKey is null ? $"r{i}" : ToIdPart(rowKey);

                // An editing row's fields are named rows[key].Field, so several editing rows
                // post apart.
                templateInfo.HtmlFieldPrefix = editing
                    ? templateInfo.GetFullHtmlFieldName($"rows[{rowKey}]")
                    : priorPrefix;

                // Row pass: re-execute the grid's child content; columns deposit their cells.
                await output.GetChildContentAsync(useCachedResult: false);
                templateInfo.HtmlFieldPrefix = priorPrefix;

                var rowBuilder = new TagBuilder("tr");
                rowBuilder.Attributes.Add("data-slot", "table-row");
                rowBuilder.Attributes.Add("class", "sa-table-row");
                if (rowKey is not null)
                {
                    rowBuilder.Attributes.Add("data-key", rowKey);
                }
                if (editing)
                {
                    rowBuilder.Attributes.Add("data-editing", "");
                }

                // An editing row has no selection checkbox.
                if (gridContext.Selection is not null && editing)
                {
                    rowBuilder.InnerHtml.AppendHtml(BuildCell());
                }
                else if (gridContext.Selection is { } selection)
                {
                    var key = rowKey ?? "";

                    var checkboxAttributes = new List<TagHelperAttribute>
                    {
                        new TagHelperAttribute("type", "checkbox"),
                        new TagHelperAttribute("data-row-select"),
                        new TagHelperAttribute("value", key),
                        new TagHelperAttribute("aria-label", $"Select row {key}"),
                    };
                    if (selection.Name is not null)
                    {
                        checkboxAttributes.Add(new TagHelperAttribute("name", selection.Name));
                    }

                    var selectCellBuilder = BuildCell();
                    selectCellBuilder.InnerHtml.AppendHtml(
                        await RenderCheckbox(context, checkboxAttributes)
                    );
                    rowBuilder.InnerHtml.AppendHtml(selectCellBuilder);
                }

                var rowDetail = gridContext.RowDetail;
                var expanded =
                    rowKey is not null && rowDetail?.ExpandedKeys.Contains(rowKey) == true;
                var detailRowId = rowKey is null ? null : $"{gridId}-detail-{ToIdPart(rowKey)}";
                if (rowDetail is { Toggle: DataGridRowDetailToggle.Start })
                {
                    rowBuilder.InnerHtml.AppendHtml(
                        await RenderToggleCell(
                            context,
                            "td",
                            "table-cell",
                            "sa-table-cell",
                            all: false,
                            expanded,
                            detailRowId,
                            hidden: editing
                        )
                    );
                }

                foreach (var cell in currentRow.Cells)
                {
                    var cellBuilder = BuildCell(cell.CssClass);
                    cellBuilder.InnerHtml.AppendHtml(cell.Html);
                    rowBuilder.InnerHtml.AppendHtml(cellBuilder);
                }

                if (rowDetail is { Toggle: DataGridRowDetailToggle.End })
                {
                    rowBuilder.InnerHtml.AppendHtml(
                        await RenderToggleCell(
                            context,
                            "td",
                            "table-cell",
                            "sa-table-cell",
                            all: false,
                            expanded,
                            detailRowId,
                            hidden: editing
                        )
                    );
                }

                rowsBuilder.AppendHtml(rowBuilder);

                // An editing row has no details row: the page keeps the one it had.
                if (rowDetail is not null && !editing)
                {
                    var detailRowBuilder = new TagBuilder("tr");
                    foreach (
                        var (name, value) in TableRowDetailRendering.GetRowAttributes(expanded)
                    )
                    {
                        detailRowBuilder.Attributes.Add(name, value);
                    }
                    detailRowBuilder.Attributes.Add("class", "sa-table-row-detail");
                    if (detailRowId is not null)
                    {
                        detailRowBuilder.Attributes.Add("id", detailRowId);
                    }
                    detailRowBuilder.InnerHtml.AppendHtml(
                        TableRowDetailRendering.BuildCell(
                            new HtmlString(currentRow.DetailHtml ?? ""),
                            GetColumnCount(gridContext)
                        )
                    );
                    rowsBuilder.AppendHtml(detailRowBuilder);
                }
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

            templateInfo.HtmlFieldPrefix = priorPrefix;
            gridContext.CurrentRow = null;
            idDiscriminator.Value = null;
        }

        return rowsBuilder;
    }

    private static TagBuilder BuildCell(string? cssClass = null)
    {
        var cellBuilder = new TagBuilder("td");
        cellBuilder.Attributes.Add("data-slot", "table-cell");
        cellBuilder.Attributes.Add("class", JoinCssClasses("sa-table-cell", cssClass));
        return cellBuilder;
    }

    /// <summary>
    ///     The number of table columns: the data columns plus any generated selection and
    ///     row details toggle columns.
    /// </summary>
    private static int GetColumnCount(DataGridContext gridContext) =>
        gridContext.Columns.Count
        + (gridContext.Selection is null ? 0 : 1)
        + (gridContext.RowDetail is { Toggle: not DataGridRowDetailToggle.None } ? 1 : 0);

    // An id may not contain whitespace
    private static string ToIdPart(string key) =>
        string.Concat(key.Select(c => char.IsWhiteSpace(c) ? '_' : c));

    /// <summary>
    ///     Renders a cell of the row details toggle column, holding a toggle rendered by the
    ///     library toggle tag helper so it matches a hand-written
    ///     <c>&lt;sa-table-row-detail-toggle&gt;</c>.
    /// </summary>
    private async Task<IHtmlContent> RenderToggleCell(
        TagHelperContext context,
        string tagName,
        string slot,
        string cssClass,
        bool all,
        bool expanded = false,
        string? controls = null,
        bool hidden = false
    )
    {
        var toggleOutput = new TagHelperOutput(
            "sa-table-row-detail-toggle",
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        await new TableRowDetailToggleTagHelper(Options.Create(_iconOptions))
        {
            All = all,
        }.ProcessAsync(context, toggleOutput);
        toggleOutput.Attributes.SetAttribute("aria-expanded", expanded ? "true" : "false");
        if (controls is not null)
        {
            toggleOutput.Attributes.SetAttribute("aria-controls", controls);
        }

        // An editing row keeps its toggle, hidden: the details indent is measured from the
        // toggle's column.
        if (hidden)
        {
            toggleOutput.Attributes.SetAttribute("hidden", "hidden");
        }

        var cellBuilder = new TagBuilder(tagName);
        cellBuilder.Attributes.Add("data-slot", slot);
        cellBuilder.Attributes.Add("class", cssClass);
        cellBuilder.Attributes.Add("style", "width: 1px");
        cellBuilder.InnerHtml.AppendHtml(toggleOutput);
        return cellBuilder;
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
        var inputTagHelper = new InputTagHelper(_htmlGenerator, _iconOptions)
        {
            ViewContext = ViewContext,
        };
        await inputTagHelper.ProcessAsync(context, checkboxOutput);

        return checkboxOutput;
    }
}
