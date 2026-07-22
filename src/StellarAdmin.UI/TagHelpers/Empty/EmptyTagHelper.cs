using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     An empty-state container that communicates the absence of content, composed of a
///     header, media, title, description, and content subcomponents.
/// </summary>
[HtmlTargetElement("sa-empty")]
public class EmptyTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "empty");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-empty", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
