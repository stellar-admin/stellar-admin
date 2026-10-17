using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the description of a card, as muted text in the card header.
/// </summary>
[HtmlTargetElement("sa-card-description")]
public sealed class CardDescriptionTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "card-description");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-card-description", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
