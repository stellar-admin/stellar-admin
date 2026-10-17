using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the footer of a table, as a <c>&lt;tfoot&gt;</c>.
/// </summary>
/// <remarks>
///     The footer usually holds summary rows.
/// </remarks>
[HtmlTargetElement("sa-table-footer")]
public sealed class TableFooterTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "tfoot";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-footer");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-table-footer", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
