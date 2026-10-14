using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Lays out read-only display fields in a description list.
/// </summary>
[HtmlTargetElement("sa-details")]
public class DetailsTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The number of columns when the list is at least 40rem wide, from 1 to 12.
    /// </summary>
    /// <remarks>
    ///     Defaults to 1. Narrower lists use one column, unless <see cref="ColumnsBase" /> or
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
    ///     The number of columns when the list is at least 56rem wide, from 1 to 12.
    /// </summary>
    /// <remarks>
    ///     Defaults to the number of columns at 40rem.
    /// </remarks>
    [HtmlAttributeName("columns-lg")]
    public int? ColumnsLarge { get; set; }

    /// <summary>
    ///     The number of columns when the list is at least 40rem wide, from 1 to 12.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="Columns" />, otherwise the number of columns at 30rem.
    /// </remarks>
    [HtmlAttributeName("columns-md")]
    public int? ColumnsMedium { get; set; }

    /// <summary>
    ///     The number of columns when the list is at least 30rem wide, from 1 to 12.
    /// </summary>
    /// <remarks>
    ///     Defaults to the number of columns at the smallest width.
    /// </remarks>
    [HtmlAttributeName("columns-sm")]
    public int? ColumnsSmall { get; set; }

    /// <summary>
    ///     How each field arranges its label and value, unless the field sets its own.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="FieldOrientation.Vertical" />.
    /// </remarks>
    [HtmlAttributeName("orientation")]
    public FieldOrientation? Orientation { get; set; }

    /// <summary>
    ///     The visual style of the list.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="DetailsVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public DetailsVariant? Variant { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var columns = GridLayout.ResolveColumns(
            "sa-details",
            Columns,
            ColumnsBase,
            ColumnsSmall,
            ColumnsMedium,
            ColumnsLarge
        );
        var effectiveVariant = Variant ?? DetailsVariant.Default;
        SetContext(context, new GridContext { Columns = columns, Owner = this });

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "details");
        output.Attributes.SetAttribute("data-variant", effectiveVariant.GetDataAttributeText());

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-details", output.GetUserSuppliedClass())
        );

        // The wrapper is the container the column breakpoints query; the list is the grid itself,
        // so it carries the column variables, which don't inherit
        var columnsStyle = ResponsiveGrid.ColumnsStyle(columns);
        output.PreContent.SetHtmlContent(
            columnsStyle is null
                ? """<dl class="sa-details-list" data-slot="details-list">"""
                : $"""<dl class="sa-details-list" data-slot="details-list" style="{columnsStyle}">"""
        );
        output.PostContent.SetHtmlContent("</dl>");

        return Task.CompletedTask;
    }
}
