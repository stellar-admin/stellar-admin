using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the title of a card.
/// </summary>
[HtmlTargetElement("sa-card-title")]
public class CardTitleTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "card-title");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-card-title", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
