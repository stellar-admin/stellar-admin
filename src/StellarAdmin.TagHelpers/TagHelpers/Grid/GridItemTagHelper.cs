using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Places content, such as a form section or a details list, in an <c>&lt;sa-grid&gt;</c>
///     with its own column span and start.
/// </summary>
/// <remarks>
///     Fields don't need one: they set their own span and start with <c>column-span</c> and
///     <c>column-start</c>.
/// </remarks>
[HtmlTargetElement("sa-grid-item")]
public class GridItemTagHelper : StellarAdminTagHelperBase, IGridPlacement
{
    /// <summary>
    ///     The number of columns the item spans, from 1 to 12, or <c>full</c> for every column.
    /// </summary>
    /// <remarks>
    ///     Defaults to 1. Limited to the grid's column count at each width.
    /// </remarks>
    [HtmlAttributeName("column-span")]
    public string? ColumnSpan { get; set; }

    /// <summary>
    ///     The number of columns the item spans when the grid is at least 56rem wide, from 1 to
    ///     12, or <c>full</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to the span at 40rem.
    /// </remarks>
    [HtmlAttributeName("column-span-lg")]
    public string? ColumnSpanLarge { get; set; }

    /// <summary>
    ///     The number of columns the item spans when the grid is at least 40rem wide, from 1 to
    ///     12, or <c>full</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to the span at 30rem.
    /// </remarks>
    [HtmlAttributeName("column-span-md")]
    public string? ColumnSpanMedium { get; set; }

    /// <summary>
    ///     The number of columns the item spans when the grid is at least 30rem wide, from 1 to
    ///     12, or <c>full</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ColumnSpan" />.
    /// </remarks>
    [HtmlAttributeName("column-span-sm")]
    public string? ColumnSpanSmall { get; set; }

    /// <summary>
    ///     The column the item starts at in its <c>&lt;sa-grid&gt;</c>, from 1 to 12, or <c>auto</c> to follow the
    ///     previous item.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>auto</c>. Ignored at a width where the grid has one column or the item
    ///     would extend past the last column.
    /// </remarks>
    [HtmlAttributeName("column-start")]
    public string? ColumnStart { get; set; }

    /// <summary>
    ///     The column the item starts at when the grid is at least 56rem wide, from 1 to 12, or
    ///     <c>auto</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to the start at 40rem.
    /// </remarks>
    [HtmlAttributeName("column-start-lg")]
    public string? ColumnStartLarge { get; set; }

    /// <summary>
    ///     The column the item starts at when the grid is at least 40rem wide, from 1 to 12, or
    ///     <c>auto</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to the start at 30rem.
    /// </remarks>
    [HtmlAttributeName("column-start-md")]
    public string? ColumnStartMedium { get; set; }

    /// <summary>
    ///     The column the item starts at when the grid is at least 30rem wide, from 1 to 12, or
    ///     <c>auto</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ColumnStart" />.
    /// </remarks>
    [HtmlAttributeName("column-start-sm")]
    public string? ColumnStartSmall { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var grid = GetEnclosingGrid(context);
        if (grid?.Owner is not GridTagHelper)
        {
            throw new InvalidOperationException(
                "An <sa-grid-item> must be placed directly in an <sa-grid>."
            );
        }

        var placementStyle = GridLayout.PlacementStyle(grid, "sa-grid-item", this);

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "grid-item");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-grid-item", output.GetUserSuppliedClass())
        );
        if (placementStyle is not null)
        {
            output.AppendStyle(placementStyle);
        }

        return Task.CompletedTask;
    }
}
