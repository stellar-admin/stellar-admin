using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The body of a table, rendered as a <c>&lt;tbody&gt;</c>; contains the data rows.
/// </summary>
[HtmlTargetElement("sa-table-body")]
public class TableBodyTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "tbody";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-body");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-table-body", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
