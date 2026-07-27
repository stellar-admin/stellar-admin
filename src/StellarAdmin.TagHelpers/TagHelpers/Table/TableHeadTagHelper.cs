using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A header cell within a table header row, rendered as a <c>&lt;th&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-table-head")]
public class TableHeadTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "th";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-head");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-table-head", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
