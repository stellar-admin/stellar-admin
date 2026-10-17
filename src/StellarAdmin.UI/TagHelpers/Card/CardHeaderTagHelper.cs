using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the header of a card, which holds the title, description and action.
/// </summary>
[HtmlTargetElement("sa-card-header")]
public sealed class CardHeaderTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "card-header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-card-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
