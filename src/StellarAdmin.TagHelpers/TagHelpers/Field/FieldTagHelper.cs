using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Wraps a form control together with its label, description, and error message, arranging them
///     according to the chosen orientation.
/// </summary>
[HtmlTargetElement("sa-field")]
public class FieldTagHelper : StellarAdminTagHelperBase, IGridPlacement
{
    /// <summary>
    ///     The number of columns the field spans in an <c>&lt;sa-grid&gt;</c>, from 1 to 12, or
    ///     <c>full</c> for every column.
    /// </summary>
    /// <remarks>
    ///     Defaults to 1. Limited to the grid's column count at each width.
    /// </remarks>
    [HtmlAttributeName("column-span")]
    public string? ColumnSpan { get; set; }

    /// <summary>
    ///     The number of columns the field spans when the grid is at least 56rem wide, from 1 to
    ///     12, or <c>full</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to the span at 40rem.
    /// </remarks>
    [HtmlAttributeName("column-span-lg")]
    public string? ColumnSpanLarge { get; set; }

    /// <summary>
    ///     The number of columns the field spans when the grid is at least 40rem wide, from 1 to
    ///     12, or <c>full</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to the span at 30rem.
    /// </remarks>
    [HtmlAttributeName("column-span-md")]
    public string? ColumnSpanMedium { get; set; }

    /// <summary>
    ///     The number of columns the field spans when the grid is at least 30rem wide, from 1 to
    ///     12, or <c>full</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ColumnSpan" />.
    /// </remarks>
    [HtmlAttributeName("column-span-sm")]
    public string? ColumnSpanSmall { get; set; }

    /// <summary>
    ///     The column the field starts at in an <c>&lt;sa-grid&gt;</c>, from 1 to 12, or <c>auto</c> to follow the
    ///     previous item.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>auto</c>. Ignored at a width where the grid has one column or the field
    ///     would extend past the last column.
    /// </remarks>
    [HtmlAttributeName("column-start")]
    public string? ColumnStart { get; set; }

    /// <summary>
    ///     The column the field starts at when the grid is at least 56rem wide, from 1 to 12, or
    ///     <c>auto</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to the start at 40rem.
    /// </remarks>
    [HtmlAttributeName("column-start-lg")]
    public string? ColumnStartLarge { get; set; }

    /// <summary>
    ///     The column the field starts at when the grid is at least 40rem wide, from 1 to 12, or
    ///     <c>auto</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to the start at 30rem.
    /// </remarks>
    [HtmlAttributeName("column-start-md")]
    public string? ColumnStartMedium { get; set; }

    /// <summary>
    ///     The column the field starts at when the grid is at least 30rem wide, from 1 to 12, or
    ///     <c>auto</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ColumnStart" />.
    /// </remarks>
    [HtmlAttributeName("column-start-sm")]
    public string? ColumnStartSmall { get; set; }

    /// <summary>
    ///     How the field arranges its label, control, and supporting text.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="FieldOrientation.Vertical" />.
    /// </remarks>
    [HtmlAttributeName("orientation")]
    public FieldOrientation? Orientation { get; set; }

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
