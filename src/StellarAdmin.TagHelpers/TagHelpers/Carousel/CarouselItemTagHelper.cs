using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     An individual slide in a carousel.
/// </summary>
[HtmlTargetElement("sa-carousel-item")]
public class CarouselItemTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "carousel-item");
        if (!output.Attributes.ContainsName("role"))
        {
            output.Attributes.SetAttribute("role", "group");
        }

        if (!output.Attributes.ContainsName("aria-roledescription"))
        {
            output.Attributes.SetAttribute("aria-roledescription", "slide");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-carousel-item", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
