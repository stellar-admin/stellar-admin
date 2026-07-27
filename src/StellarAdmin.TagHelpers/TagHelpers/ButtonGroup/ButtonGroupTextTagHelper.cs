using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Renders a non-interactive text label within a button group.
/// </summary>
[HtmlTargetElement("sa-button-group-text")]
public class ButtonGroupTextTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "button-group-text");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-button-group-text", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
