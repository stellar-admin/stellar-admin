using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a caption that describes a table, as a <c>&lt;caption&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-table-caption")]
public class TableCaptionTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "caption";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-caption");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-table-caption", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
