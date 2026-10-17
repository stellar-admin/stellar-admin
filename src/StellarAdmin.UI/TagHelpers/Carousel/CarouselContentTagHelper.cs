using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the scrollable viewport that holds the slides of a carousel.
/// </summary>
/// <remarks>
///     Must be placed inside an <c>sa-carousel</c> element.
/// </remarks>
[HtmlTargetElement("sa-carousel-content")]
public sealed class CarouselContentTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The element isn't inside an <c>sa-carousel</c> element.
    /// </exception>
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var carouselContext =
            GetContext<CarouselContext>(context)
            ?? throw new InvalidOperationException(
                "<sa-carousel-content> must be inside <sa-carousel>."
            );

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "carousel-content");
        if (!output.Attributes.ContainsName("tabindex"))
        {
            output.Attributes.SetAttribute("tabindex", "0");
        }

        output.Attributes.SetAttribute(
            "data-orientation",
            carouselContext.Orientation.GetDataAttributeText()
        );
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-carousel-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
