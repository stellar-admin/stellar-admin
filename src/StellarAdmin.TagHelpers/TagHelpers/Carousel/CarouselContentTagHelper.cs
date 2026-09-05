using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The scrollable viewport containing carousel items.
/// </summary>
[HtmlTargetElement("sa-carousel-content")]
public class CarouselContentTagHelper : StellarAdminTagHelperBase
{
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
