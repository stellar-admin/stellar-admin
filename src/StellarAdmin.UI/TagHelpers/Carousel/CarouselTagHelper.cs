using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a scrollable set of slides with optional navigation controls.
/// </summary>
/// <remarks>
///     Requires <c>stellar-admin.js</c>.
/// </remarks>
[HtmlTargetElement("sa-carousel")]
public class CarouselTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the direction in which the slides scroll.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the scroll direction. The default is
    ///     <see cref="CarouselOrientation.Horizontal" />.
    /// </value>
    [HtmlAttributeName("orientation")]
    public CarouselOrientation? Orientation { get; set; }

    /// <inheritdoc />
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
