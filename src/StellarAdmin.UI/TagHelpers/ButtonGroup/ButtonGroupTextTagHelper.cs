using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a text label in a button group.
/// </summary>
[HtmlTargetElement("sa-button-group-text")]
public class ButtonGroupTextTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
