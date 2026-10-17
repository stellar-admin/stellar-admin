using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Lays out <c>sa-display-field</c> elements in a description list.
/// </summary>
[HtmlTargetElement("sa-details")]
public class DetailsTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the number of columns when the list is at least 40rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12. The default is the number of columns at 30rem.
    /// </value>
    /// <remarks>
    ///     The same as <c>columns-md</c>. Can't be combined with it.
    /// </remarks>
    [HtmlAttributeName("columns")]
    public int? Columns { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns at the smallest width.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12. The default is <c>1</c>.
    /// </value>
    [HtmlAttributeName("columns-base")]
    public int? ColumnsBase { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns when the list is at least 56rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12. The default is the number of columns at 40rem.
    /// </value>
    [HtmlAttributeName("columns-lg")]
    public int? ColumnsLarge { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns when the list is at least 40rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12. The default is the number of columns at 30rem.
    /// </value>
    /// <remarks>
    ///     Can't be combined with <c>columns</c>.
    /// </remarks>
    [HtmlAttributeName("columns-md")]
    public int? ColumnsMedium { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns when the list is at least 30rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12. The default is the number of columns at the smallest width.
    /// </value>
    [HtmlAttributeName("columns-sm")]
    public int? ColumnsSmall { get; set; }

    /// <summary>
    ///     Gets or sets how each field arranges its label and value.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the arrangement. The default is
    ///     <see cref="DetailsOrientation.Vertical" />.
    /// </value>
    /// <remarks>
    ///     A field's own <c>orientation</c> takes precedence.
    /// </remarks>
    [HtmlAttributeName("orientation")]
    public DetailsOrientation? Orientation { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the list.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the visual style. The default is
    ///     <see cref="DetailsVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public DetailsVariant? Variant { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     <see cref="Columns" /> and <see cref="ColumnsMedium" /> are both set, or a column count
    ///     is less than 1 or greater than 12.
    /// </exception>
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
