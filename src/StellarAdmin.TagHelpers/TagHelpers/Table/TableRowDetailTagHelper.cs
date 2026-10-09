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

        foreach (var (name, value) in TableRowDetailRendering.GetRowAttributes(expanded))
        {
            output.Attributes.SetAttribute(name, value);
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-table-row-detail", output.GetUserSuppliedClass())
        );

        var cellBuilder = TableRowDetailRendering.BuildCell(
            await output.GetChildContentAsync(),
            Colspan
        );
        output.Content.SetHtmlContent(cellBuilder);
    }
}
