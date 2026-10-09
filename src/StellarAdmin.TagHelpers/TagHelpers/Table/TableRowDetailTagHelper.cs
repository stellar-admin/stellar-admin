using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The details of the table row before it, rendered as a <c>&lt;tr&gt;</c> with one cell
///     spanning every column. It is collapsed until the row's
///     <c>sa-table-row-detail-toggle</c> expands it.
/// </summary>
[HtmlTargetElement("sa-table-row-detail")]
public class TableRowDetailTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Whether the details render expanded. Expanding a row in the browser raises
    ///     <c>row-detail-expand</c>; a row rendered expanded does not, so render its content
    ///     directly rather than loading it on expand.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    [HtmlAttributeName("expanded")]
    public bool? Expanded { get; set; }

    /// <summary>
    ///     The number of columns the details span. When omitted, the details span every column
    ///     of the table's header row.
    /// </summary>
    [HtmlAttributeName("colspan")]
    public int? Colspan { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var expanded = Expanded ?? false;

        output.TagName = "tr";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-row-detail");
        output.Attributes.SetAttribute("data-state", expanded ? "open" : "closed");
        if (!expanded)
        {
            output.Attributes.SetAttribute("hidden", "hidden");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-table-row-detail", output.GetUserSuppliedClass())
        );

        // The reveal wrapper animates grid-template-rows to the content's height, the clip
        // hides the content while it collapses, and the content box stays pinned to the
        // visible width when the table scrolls sideways.
        var contentBuilder = new TagBuilder("div");
        contentBuilder.Attributes.Add("data-slot", "table-row-detail-content");
        contentBuilder.Attributes.Add("class", "sa-table-row-detail-content");
        contentBuilder.InnerHtml.AppendHtml(await output.GetChildContentAsync());

        var clipBuilder = new TagBuilder("div");
        clipBuilder.Attributes.Add("class", "sa-table-row-detail-clip");
        clipBuilder.InnerHtml.AppendHtml(contentBuilder);

        var revealBuilder = new TagBuilder("div");
        revealBuilder.Attributes.Add("class", "sa-table-row-detail-reveal");
        revealBuilder.InnerHtml.AppendHtml(clipBuilder);

        var cellBuilder = new TagBuilder("td");
        cellBuilder.Attributes.Add("data-slot", "table-row-detail-cell");
        cellBuilder.Attributes.Add("class", "sa-table-row-detail-cell");
        if (Colspan is { } colspan)
        {
            cellBuilder.Attributes.Add("colspan", colspan.ToString(CultureInfo.InvariantCulture));
        }

        cellBuilder.InnerHtml.AppendHtml(revealBuilder);

        output.Content.SetHtmlContent(cellBuilder);
    }
}
