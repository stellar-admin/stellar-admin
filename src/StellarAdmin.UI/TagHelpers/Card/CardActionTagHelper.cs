using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an area in the top-right corner of a card header for a button or other control.
/// </summary>
[HtmlTargetElement("sa-card-action")]
public sealed class CardActionTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
