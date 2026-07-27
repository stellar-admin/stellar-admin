using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The header region of an empty state; typically contains the media, title, and description.
/// </summary>
[HtmlTargetElement("sa-empty-header")]
public class EmptyHeaderTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "empty-header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-empty-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
