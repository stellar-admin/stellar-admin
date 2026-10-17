using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the title in an empty state's header.
/// </summary>
[HtmlTargetElement("sa-empty-title")]
public class EmptyTitleTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "empty-title");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-empty-title", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
