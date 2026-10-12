using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Lays out fields and other content in responsive columns.
/// </summary>
/// <remarks>
///     Fields and display fields placed directly in the grid set their own span with
///     <c>column-span</c>; other content spans through an <c>&lt;sa-grid-item&gt;</c>.
/// </remarks>
[HtmlTargetElement("sa-grid")]
public class GridTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The number of columns when the grid is at least 40rem wide, from 1 to 12.
    /// </summary>
    /// <remarks>
    ///     Defaults to 1. Narrower grids use one column, unless <see cref="ColumnsBase" /> or
    ///     <see cref="ColumnsSmall" /> is set.
    /// </remarks>
    [HtmlAttributeName("columns")]
    public int? Columns { get; set; }

    /// <summary>
    ///     The number of columns at the smallest width, from 1 to 12.
    /// </summary>
    /// <remarks>
    ///     Defaults to 1.
    /// </remarks>
    [HtmlAttributeName("columns-base")]
    public int? ColumnsBase { get; set; }

    /// <summary>
    ///     The number of columns when the grid is at least 56rem wide, from 1 to 12.
    /// </summary>
    /// <remarks>
    ///     Defaults to the number of columns at 40rem.
    /// </remarks>
    [HtmlAttributeName("columns-lg")]
    public int? ColumnsLarge { get; set; }

    /// <summary>
    ///     The number of columns when the grid is at least 40rem wide, from 1 to 12.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="Columns" />, otherwise the number of columns at 30rem.
    /// </remarks>
    [HtmlAttributeName("columns-md")]
    public int? ColumnsMedium { get; set; }

    /// <summary>
    ///     The number of columns when the grid is at least 30rem wide, from 1 to 12.
    /// </summary>
    /// <remarks>
    ///     Defaults to the number of columns at the smallest width.
    /// </remarks>
    [HtmlAttributeName("columns-sm")]
    public int? ColumnsSmall { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var columns = GridLayout.ResolveColumns(
            "sa-grid",
            Columns,
            ColumnsBase,
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
