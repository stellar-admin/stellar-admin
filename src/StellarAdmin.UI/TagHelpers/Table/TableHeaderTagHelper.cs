using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the header of a table, which holds the header row, as a <c>&lt;thead&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-table-header")]
public sealed class TableHeaderTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
