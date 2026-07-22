using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     An action region within a card header, aligned to the top-right corner; typically
///     contains a button or other interactive control.
/// </summary>
[HtmlTargetElement("sa-card-action")]
public class CardActionTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "card-action");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-card-action", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
