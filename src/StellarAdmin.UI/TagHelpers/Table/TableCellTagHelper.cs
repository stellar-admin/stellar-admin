using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a data cell in a table row, as a <c>&lt;td&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-table-cell")]
public sealed class TableCellTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "td";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-cell");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-table-cell", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
