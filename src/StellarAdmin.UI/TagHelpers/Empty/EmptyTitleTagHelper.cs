using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The title text within an empty state header.
/// </summary>
[HtmlTargetElement("sa-empty-title")]
public class EmptyTitleTagHelper : StellarAdminTagHelperBase
{
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
