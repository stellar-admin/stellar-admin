using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Places content, such as a form section or a details list, in an <c>&lt;sa-grid&gt;</c>
///     with its own column span.
/// </summary>
/// <remarks>
///     Fields don't need one: they set their own span with <c>column-span</c>.
/// </remarks>
[HtmlTargetElement("sa-grid-item")]
public class GridItemTagHelper : StellarAdminTagHelperBase
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

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var grid = GetEnclosingGrid(context);
        if (grid?.Owner is not GridTagHelper)
        {
            throw new InvalidOperationException(
                "An <sa-grid-item> must be placed directly in an <sa-grid>."
            );
        }

        var spanStyle = GridLayout.SpanStyle(
            grid,
            "sa-grid-item",
            ColumnSpan,
            ColumnSpanSmall,
            ColumnSpanMedium,
            ColumnSpanLarge
        );

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "grid-item");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-grid-item", output.GetUserSuppliedClass())
        );
        if (spanStyle is not null)
        {
            output.AppendStyle(spanStyle);
        }

        return Task.CompletedTask;
    }
}
