using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The header region of a popover; typically contains the title and description.
/// </summary>
[HtmlTargetElement("sa-popover-header")]
public class PopoverHeaderTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "popover-header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-popover-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
