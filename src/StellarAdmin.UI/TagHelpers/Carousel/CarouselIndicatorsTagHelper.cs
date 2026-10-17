using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders dots that scroll a carousel to each of its positions.
/// </summary>
[HtmlTargetElement("sa-carousel-indicators")]
public sealed class CarouselIndicatorsTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "carousel-indicators");
        output.Attributes.SetAttribute("role", "group");
        if (!output.Attributes.ContainsName("aria-label"))
        {
            output.Attributes.SetAttribute("aria-label", "Choose a slide");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-carousel-indicators", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
