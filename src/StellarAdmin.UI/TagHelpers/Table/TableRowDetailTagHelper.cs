using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the details of the table row before it.
/// </summary>
/// <remarks>
///     The details are a row with one cell that spans the columns. They stay collapsed until the
///     row's <c>&lt;sa-table-row-detail-toggle&gt;</c> expands them. Expanding and collapsing
///     the details raises the bubbling <c>rowdetailexpand</c> and <c>rowdetailcollapse</c>
///     events on this row.
/// </remarks>
[HtmlTargetElement("sa-table-row-detail")]
public sealed class TableRowDetailTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets a value that indicates whether the details are expanded when the page loads.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the details are expanded; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Details that start expanded don't raise <c>rowdetailexpand</c>, so render their content
    ///     directly instead of loading it when they expand.
    /// </remarks>
    [HtmlAttributeName("expanded")]
    public bool? Expanded { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns that the details span.
    /// </summary>
    /// <value>
    ///     The number of columns. The default spans every column of the table's header row.
    /// </value>
    [HtmlAttributeName("colspan")]
    public int? Colspan { get; set; }

    /// <inheritdoc />
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
