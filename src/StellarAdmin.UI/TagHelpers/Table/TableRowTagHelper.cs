using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A row within a table, rendered as a <c>&lt;tr&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-table-row")]
public class TableRowTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "tr";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-row");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-table-row", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
