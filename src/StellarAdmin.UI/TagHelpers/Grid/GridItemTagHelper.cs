using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Places content, such as a form section or a details list, in an <c>&lt;sa-grid&gt;</c>
///     with its own column span and start.
/// </summary>
/// <remarks>
///     The item must be placed directly in an <c>&lt;sa-grid&gt;</c>. Fields don't need one,
///     because they set their own span and start with <c>column-span</c> and <c>column-start</c>.
/// </remarks>
[HtmlTargetElement("sa-grid-item")]
public class GridItemTagHelper : StellarAdminTagHelperBase, IGridPlacement
{
    /// <summary>
    ///     Gets or sets the number of columns the item spans in an <c>&lt;sa-grid&gt;</c>.
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
    ///     Gets or sets the number of columns the item spans when the grid is at least 56rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>full</c> for every column. The default is the span at
    ///     40rem.
    /// </value>
    [HtmlAttributeName("column-span-lg")]
    public string? ColumnSpanLarge { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns the item spans when the grid is at least 40rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>full</c> for every column. The default is the span at
    ///     30rem.
    /// </value>
    [HtmlAttributeName("column-span-md")]
    public string? ColumnSpanMedium { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns the item spans when the grid is at least 30rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>full</c> for every column. The default is the span at the
    ///     smallest width.
    /// </value>
    [HtmlAttributeName("column-span-sm")]
    public string? ColumnSpanSmall { get; set; }

    /// <summary>
    ///     Gets or sets the column the item starts at in an <c>&lt;sa-grid&gt;</c>.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>auto</c> to follow the previous item. The default is
    ///     <c>auto</c>.
    /// </value>
    /// <remarks>
    ///     Ignored at a width where the grid has one column or the item would extend past the last
    ///     column.
    /// </remarks>
    [HtmlAttributeName("column-start")]
    public string? ColumnStart { get; set; }

    /// <summary>
    ///     Gets or sets the column the item starts at when the grid is at least 56rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>auto</c> to follow the previous item. The default is the
    ///     start at 40rem.
    /// </value>
    /// <remarks>
    ///     Ignored at a width where the grid has one column or the item would extend past the last
    ///     column.
    /// </remarks>
    [HtmlAttributeName("column-start-lg")]
    public string? ColumnStartLarge { get; set; }

    /// <summary>
    ///     Gets or sets the column the item starts at when the grid is at least 40rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>auto</c> to follow the previous item. The default is the
    ///     start at 30rem.
    /// </value>
    /// <remarks>
    ///     Ignored at a width where the grid has one column or the item would extend past the last
    ///     column.
    /// </remarks>
    [HtmlAttributeName("column-start-md")]
    public string? ColumnStartMedium { get; set; }

    /// <summary>
    ///     Gets or sets the column the item starts at when the grid is at least 30rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>auto</c> to follow the previous item. The default is the
    ///     start at the smallest width.
    /// </value>
    /// <remarks>
    ///     Ignored at a width where the grid has one column or the item would extend past the last
    ///     column.
    /// </remarks>
    [HtmlAttributeName("column-start-sm")]
    public string? ColumnStartSmall { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The item isn't placed directly in an <c>&lt;sa-grid&gt;</c>, or a span or start isn't
    ///     valid.
    /// </exception>
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
