using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a header cell in a table's header row, as a <c>&lt;th&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-table-head")]
public sealed class TableHeadTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
