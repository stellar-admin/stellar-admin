using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the footer of a card, which holds actions or extra information.
/// </summary>
[HtmlTargetElement("sa-card-footer")]
public class CardFooterTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "card-footer");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-card-footer", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
