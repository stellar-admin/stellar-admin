using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the body of a table, which holds the data rows, as a <c>&lt;tbody&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-table-body")]
public class TableBodyTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
