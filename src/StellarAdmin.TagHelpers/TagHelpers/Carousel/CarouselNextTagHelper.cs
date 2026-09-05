using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A button that scrolls to the next carousel position.
/// </summary>
[HtmlTargetElement("sa-carousel-next")]
public class CarouselNextTagHelper : StellarAdminTagHelperBase
{
    private readonly IIconManager _iconManager;

    /// <summary>
    ///     The button size.
    /// </summary>
    /// <remarks>
    ///     Defaults to IconSmall.
    /// </remarks>
    public ButtonSize? Size { get; set; }

    /// <summary>
    ///     The button appearance.
    /// </summary>
    /// <remarks>
    ///     Defaults to Outline.
    /// </remarks>
    public ButtonVariant? Variant { get; set; }

    public CarouselNextTagHelper(IIconManager iconManager)
    {
        _iconManager = iconManager;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var carouselContext =
            GetContext<CarouselContext>(context)
            ?? throw new InvalidOperationException(
                "<sa-carousel-next> must be inside <sa-carousel>."
            );

        var effectiveSize = Size ?? ButtonSize.IconSmall;
        var effectiveVariant = Variant ?? ButtonVariant.Outline;

        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (!output.Attributes.ContainsName("type"))
        {
            output.Attributes.SetAttribute("type", "button");
        }

        output.Attributes.SetAttribute("data-slot", "carousel-next");
        if (!output.Attributes.ContainsName("aria-label"))
        {
            output.Attributes.SetAttribute("aria-label", "Next slide");
        }

        output.Attributes.SetAttribute("command", "--carousel-next");
        output.Attributes.SetAttribute("commandfor", carouselContext.CarouselId);
        output.Attributes.SetAttribute("disabled", "disabled");
        output.Attributes.SetAttribute(
            "data-orientation",
            carouselContext.Orientation.GetDataAttributeText()
        );
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-carousel-next", output.GetUserSuppliedClass())
        );

        ButtonRenderingHelper.RenderAttributes(output, effectiveVariant, effectiveSize);

        var content = await output.GetChildContentAsync();
        if (content.IsEmptyOrWhiteSpace)
        {
            var icon = new TagHelperOutput(
                "svg",
                [new TagHelperAttribute("class", "size-4")],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );

            new IconTagHelper(_iconManager) { Name = "chevron-right" }.Process(context, icon);

            output.Content.SetHtmlContent(icon);
        }
        else
        {
            output.Content.SetHtmlContent(content);
        }
    }
}
