using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The header section of a table, rendered as a <c>&lt;thead&gt;</c>; contains the header row.
/// </summary>
[HtmlTargetElement("sa-table-header")]
public class TableHeaderTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "thead";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-table-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
