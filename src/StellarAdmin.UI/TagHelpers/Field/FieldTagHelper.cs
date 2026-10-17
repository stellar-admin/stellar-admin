using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a field that groups a form control with its label, description and error message.
/// </summary>
[HtmlTargetElement("sa-field")]
public sealed class FieldTagHelper : StellarAdminTagHelperBase, IGridPlacement
{
    /// <summary>
    ///     Gets or sets the number of columns the field spans in an <c>&lt;sa-grid&gt;</c>.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>full</c> for every column. The default is <c>1</c>.
    /// </value>
    /// <remarks>
    ///     The span is limited to the grid's column count at each width.
    /// </remarks>
    [HtmlAttributeName("column-span")]
    public string? ColumnSpan { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns the field spans when the grid is at least 56rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>full</c> for every column. The default is the span at
    ///     40rem.
    /// </value>
    [HtmlAttributeName("column-span-lg")]
    public string? ColumnSpanLarge { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns the field spans when the grid is at least 40rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>full</c> for every column. The default is the span at
    ///     30rem.
    /// </value>
    [HtmlAttributeName("column-span-md")]
    public string? ColumnSpanMedium { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns the field spans when the grid is at least 30rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>full</c> for every column. The default is the span at the
    ///     smallest width.
    /// </value>
    [HtmlAttributeName("column-span-sm")]
    public string? ColumnSpanSmall { get; set; }

    /// <summary>
    ///     Gets or sets the column the field starts at in an <c>&lt;sa-grid&gt;</c>.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>auto</c> to follow the previous item. The default is
    ///     <c>auto</c>.
    /// </value>
    /// <remarks>
    ///     Ignored at a width where the grid has one column or the field would extend past the last
    ///     column.
    /// </remarks>
    [HtmlAttributeName("column-start")]
    public string? ColumnStart { get; set; }

    /// <summary>
    ///     Gets or sets the column the field starts at when the grid is at least 56rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>auto</c> to follow the previous item. The default is the
    ///     start at 40rem.
    /// </value>
    /// <remarks>
    ///     Ignored at a width where the grid has one column or the field would extend past the last
    ///     column.
    /// </remarks>
    [HtmlAttributeName("column-start-lg")]
    public string? ColumnStartLarge { get; set; }

    /// <summary>
    ///     Gets or sets the column the field starts at when the grid is at least 40rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>auto</c> to follow the previous item. The default is the
    ///     start at 30rem.
    /// </value>
    /// <remarks>
    ///     Ignored at a width where the grid has one column or the field would extend past the last
    ///     column.
    /// </remarks>
    [HtmlAttributeName("column-start-md")]
    public string? ColumnStartMedium { get; set; }

    /// <summary>
    ///     Gets or sets the column the field starts at when the grid is at least 30rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>auto</c> to follow the previous item. The default is the
    ///     start at the smallest width.
    /// </value>
    /// <remarks>
    ///     Ignored at a width where the grid has one column or the field would extend past the last
    ///     column.
    /// </remarks>
    [HtmlAttributeName("column-start-sm")]
    public string? ColumnStartSmall { get; set; }

    /// <summary>
    ///     Gets or sets how the field arranges its label, control and supporting text.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the arrangement. The default is
    ///     <see cref="FieldOrientation.Vertical" />.
    /// </value>
    [HtmlAttributeName("orientation")]
    public FieldOrientation? Orientation { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     A column span or start isn't valid.
    /// </exception>
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // The MergeAttributes call below does a simple concatenation of the classes, but this is not correct.
        // We need to compose the class string, whil happen inside the FieldTagBuilder.
        // To ensure correct behaviour we need to first extract the user supplied class so we can pass it to
        // FieldTagBuilder and then clear it before calling MergeAttribute() to prevent a double merge.
        var userSuppliedClass = output.GetUserSuppliedClass();
        output.Attributes.SetAttribute("class", string.Empty);

        var effectiveOrientation = Orientation ?? FieldOrientation.Vertical;
        var grid = GetEnclosingGrid(context);
        var placementStyle = GridLayout.PlacementStyle(grid, "sa-field", this);

        var tagBuilder = new FieldTagBuilder(effectiveOrientation, userSuppliedClass);

        output.TagName = tagBuilder.TagName;
        output.TagMode = TagMode.StartTagAndEndTag;

        output.MergeAttributes(tagBuilder);

        // Directly in a grid, the field sits in a grid item that carries its placement
        if (grid is not null)
        {
            GridLayout.WrapInItem(output, placementStyle);
        }

        return Task.CompletedTask;
    }
}
