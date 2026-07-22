using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The main content region of a card.
/// </summary>
[HtmlTargetElement("sa-card-content")]
public class CardContentTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "card-content");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-card-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
