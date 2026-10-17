using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Lays out fields and other content in responsive columns.
/// </summary>
/// <remarks>
///     Breakpoints are measured against the width of the grid. Fields and display fields placed
///     directly in the grid set their own span with <c>column-span</c>. Other content spans
///     through an <c>&lt;sa-grid-item&gt;</c>.
/// </remarks>
[HtmlTargetElement("sa-grid")]
public sealed class GridTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the number of columns at the smallest width.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12. The default is <c>1</c>.
    /// </value>
    /// <remarks>
    ///     Wider breakpoints use this count unless they set their own.
    /// </remarks>
    [HtmlAttributeName("columns")]
    public int? Columns { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns when the grid is at least 56rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12. The default is the number of columns at 40rem.
    /// </value>
    [HtmlAttributeName("columns-lg")]
    public int? ColumnsLarge { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns when the grid is at least 40rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12. The default is the number of columns at 30rem.
    /// </value>
    [HtmlAttributeName("columns-md")]
    public int? ColumnsMedium { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns when the grid is at least 30rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12. The default is the number of columns at the smallest width.
    /// </value>
    [HtmlAttributeName("columns-sm")]
    public int? ColumnsSmall { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     A column count is outside 1 to 12.
    /// </exception>
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var columns = GridLayout.ResolveColumns(
            "sa-grid",
            Columns,
            ColumnsSmall,
            ColumnsMedium,
            ColumnsLarge
        );
        SetContext(context, new GridContext { Columns = columns, Owner = this });

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "grid");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-grid", output.GetUserSuppliedClass())
        );

        // The wrapper is the container the column breakpoints query; the content inside is the
        // grid itself, so it carries the column variables, which don't inherit. A grid with one
        // column at every width stacks its items, which keeps the rule between stacked sections.
        var columnsStyle = ResponsiveGrid.ColumnsStyle(columns);
        var columnsKind = columnsStyle is null ? "single" : "multiple";
        output.PreContent.SetHtmlContent(
            columnsStyle is null
                ? $"""<div class="sa-grid-content" data-slot="grid-content" data-columns="{columnsKind}">"""
                : $"""<div class="sa-grid-content" data-slot="grid-content" data-columns="{columnsKind}" style="{columnsStyle}">"""
        );
        output.PostContent.SetHtmlContent("</div>");

        return Task.CompletedTask;
    }
}
