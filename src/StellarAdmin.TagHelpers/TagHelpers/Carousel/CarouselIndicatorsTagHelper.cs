using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Navigation dots for the distinct scroll positions in a carousel.
/// </summary>
[HtmlTargetElement("sa-carousel-indicators")]
public class CarouselIndicatorsTagHelper : StellarAdminTagHelperBase
{
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
