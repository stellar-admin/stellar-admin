using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an image as a thumbnail or a figure.
/// </summary>
/// <remarks>
///     A placeholder is shown when there's no image, and a broken-image tile when it can't be
///     loaded. The broken-image tile, lightbox and preview require <c>stellar-admin.js</c>.
/// </remarks>
[HtmlTargetElement("sa-image", TagStructure = TagStructure.WithoutEndTag)]
public class ImageTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ImageTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the image's icons.</param>
    public ImageTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Gets or sets the image's text alternative.
    /// </summary>
    /// <remarks>
    ///     The text also names the button that opens the lightbox.
    /// </remarks>
    [HtmlAttributeName("alt")]
    public string? Alt { get; set; }

    /// <summary>
    ///     Gets or sets the shape of the box the image is shown in.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the shape. The default is square for a
    ///     thumbnail and the image's own shape for a figure.
    /// </value>
    [HtmlAttributeName("aspect-ratio")]
    public ImageAspectRatio? AspectRatio { get; set; }

    /// <summary>
    ///     Gets or sets how the image fills a box of another shape.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the fit. The default is
    ///     <see cref="ImageFit.Cover" />.
    /// </value>
    /// <remarks>
    ///     Ignored when <see cref="AspectRatio" /> is <see cref="ImageAspectRatio.Original" />.
    /// </remarks>
    [HtmlAttributeName("fit")]
    public ImageFit? Fit { get; set; }

    /// <summary>
    ///     Gets or sets what pointing at or clicking the image does.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the interaction. The default is
    ///     <see cref="ImageInteraction.Lightbox" />.
    /// </value>
    [HtmlAttributeName("interaction")]
    public ImageInteraction? Interaction { get; set; }

    /// <summary>
    ///     Gets or sets the image's height, which is a thumbnail's height or a figure's maximum
    ///     height.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the height. The default is small for a
    ///     thumbnail and medium for a figure.
    /// </value>
    [HtmlAttributeName("size")]
    public ImageSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets the URL of the image.
    /// </summary>
    /// <remarks>
    ///     A placeholder is shown when the URL is empty.
    /// </remarks>
    [HtmlAttributeName("src")]
    public string? Source { get; set; }

    /// <summary>
    ///     Gets or sets where the image is shown, which sets its sizes and its empty and error states.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the variant. The default is
    ///     <see cref="ImageVariant.Thumbnail" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public ImageVariant? Variant { get; set; }

    /// <summary>
    ///     Gets or sets the context of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext? ViewContext { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? ImageVariant.Thumbnail;
        var isFigure = effectiveVariant == ImageVariant.Figure;
        var appearance = new ImageAppearance(
            effectiveVariant,
            Size ?? (isFigure ? ImageSize.Medium : ImageSize.Small),
            AspectRatio ?? (isFigure ? ImageAspectRatio.Original : ImageAspectRatio.Square),
            Fit ?? ImageFit.Cover,
            Interaction ?? ImageInteraction.Lightbox
        );

        var image = await ImageRendering.RenderImageAsync(
            context,
            _iconOptions,
            Source,
            Alt,
            appearance
        );

        output.TagName = image.TagName;
        output.TagMode = TagMode.StartTagAndEndTag;
        var userClass = output.GetUserSuppliedClass();
        foreach (var (name, value) in image.Attributes)
        {
            output.Attributes.SetAttribute(name, value);
        }

        output.Attributes.SetAttribute("class", JoinCssClasses("sa-image", userClass));
        output.Content.SetHtmlContent(image.InnerHtml);

        if (image.TagName == "button")
        {
            output.PostElement.AppendHtml(
                await ImageRendering.RenderLightboxTemplateOnceAsync(
                    context,
                    _iconOptions,
                    ViewContext?.HttpContext
                )
            );
        }
    }
}
