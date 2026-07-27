using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The footer of a table, rendered as a <c>&lt;tfoot&gt;</c>; typically holds summary rows.
/// </summary>
[HtmlTargetElement("sa-table-footer")]
public class TableFooterTagHelper : StellarAdminTagHelperBase
{
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
