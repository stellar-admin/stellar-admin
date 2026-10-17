using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a button that scrolls a carousel to the previous position.
/// </summary>
/// <remarks>
///     Must be placed inside an <c>sa-carousel</c> element. Shows an arrow icon when the element
///     has no content.
/// </remarks>
[HtmlTargetElement("sa-carousel-previous")]
public class CarouselPreviousTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Gets or sets the size of the button.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size of the button. The default is
    ///     <see cref="ButtonSize.IconSmall" />.
    /// </value>
    [HtmlAttributeName("size")]
    public ButtonSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the button.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the visual style of the button. The
    ///     default is <see cref="ButtonVariant.Outline" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public ButtonVariant? Variant { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CarouselPreviousTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the arrow icon.</param>
    public CarouselPreviousTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The element isn't inside an <c>sa-carousel</c> element.
    /// </exception>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var carouselContext =
            GetContext<CarouselContext>(context)
            ?? throw new InvalidOperationException(
                "<sa-carousel-previous> must be inside <sa-carousel>."
            );

        var effectiveSize = Size ?? ButtonSize.IconSmall;
        var effectiveVariant = Variant ?? ButtonVariant.Outline;

        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (!output.Attributes.ContainsName("type"))
        {
            output.Attributes.SetAttribute("type", "button");
        }

        output.Attributes.SetAttribute("data-slot", "carousel-previous");
        if (!output.Attributes.ContainsName("aria-label"))
        {
            output.Attributes.SetAttribute("aria-label", "Previous slide");
        }

        output.Attributes.SetAttribute("command", "--carousel-previous");
        output.Attributes.SetAttribute("commandfor", carouselContext.CarouselId);
        output.Attributes.SetAttribute("disabled", "disabled");
        output.Attributes.SetAttribute(
            "data-orientation",
            carouselContext.Orientation.GetDataAttributeText()
        );
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-carousel-previous", output.GetUserSuppliedClass())
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

            new IconTagHelper(_iconOptions)
            {
                Name = _iconOptions.GetSemanticIconName(SemanticIconRole.CarouselPrevious),
            }.Process(context, icon);

            output.Content.SetHtmlContent(icon);
        }
        else
        {
            output.Content.SetHtmlContent(content);
        }
    }
}
