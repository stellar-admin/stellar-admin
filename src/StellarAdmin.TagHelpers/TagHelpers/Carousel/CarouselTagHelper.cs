using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A scrollable collection of slides with optional navigation controls.
/// </summary>
[HtmlTargetElement("sa-carousel")]
public class CarouselTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The direction in which items scroll.
    /// </summary>
    /// <remarks>
    ///     Defaults to Horizontal.
    /// </remarks>
    public CarouselOrientation? Orientation { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveOrientation = Orientation ?? CarouselOrientation.Horizontal;
        var carouselId =
            output.Attributes["id"]?.Value?.ToString() ?? $"carousel-{Guid.NewGuid():N}";

        SetContext(
            context,
            new CarouselContext { CarouselId = carouselId, Orientation = effectiveOrientation }
        );

        output.TagName = "sel-carousel";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("id", carouselId);
        output.Attributes.SetAttribute("data-slot", "carousel");
        output.Attributes.SetAttribute(
            "data-orientation",
            effectiveOrientation.GetDataAttributeText()
        );
        if (!output.Attributes.ContainsName("role"))
        {
            output.Attributes.SetAttribute("role", "region");
        }

        if (!output.Attributes.ContainsName("aria-roledescription"))
        {
            output.Attributes.SetAttribute("aria-roledescription", "carousel");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-carousel", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
