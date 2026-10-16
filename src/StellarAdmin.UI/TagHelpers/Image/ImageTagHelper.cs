using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Displays an image as a thumbnail or a figure, with a placeholder when there is none and a
///     broken-image tile when it can't be loaded.
/// </summary>
[HtmlTargetElement("sa-image", TagStructure = TagStructure.WithoutEndTag)]
public class ImageTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    public ImageTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     The image's text alternative, which also names the button that opens the lightbox.
    /// </summary>
    [HtmlAttributeName("alt")]
    public string? Alt { get; set; }

    /// <summary>
    ///     The shape of the box the image is shown in.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ImageAspectRatio.Square" /> for a thumbnail and
    ///     <see cref="ImageAspectRatio.Original" /> for a figure.
    /// </remarks>
    [HtmlAttributeName("aspect-ratio")]
    public ImageAspectRatio? AspectRatio { get; set; }

    /// <summary>
    ///     How the image fills a box of another shape. Ignored for
    ///     <see cref="ImageAspectRatio.Original" />.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ImageFit.Cover" />.
    /// </remarks>
    [HtmlAttributeName("fit")]
    public ImageFit? Fit { get; set; }

    /// <summary>
    ///     What pointing at or clicking the image does.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ImageInteraction.Lightbox" />.
    /// </remarks>
    [HtmlAttributeName("interaction")]
    public ImageInteraction? Interaction { get; set; }

    /// <summary>
    ///     The image's height: a thumbnail's height, or a figure's maximum height.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ImageSize.Small" /> for a thumbnail and
    ///     <see cref="ImageSize.Medium" /> for a figure.
    /// </remarks>
    [HtmlAttributeName("size")]
    public ImageSize? Size { get; set; }

    /// <summary>
    ///     The URL of the image. When empty, a placeholder is shown.
    /// </summary>
    [HtmlAttributeName("src")]
    public string? Source { get; set; }

    /// <summary>
    ///     Where the image is shown, which sets its sizes and empty and error states.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ImageVariant.Thumbnail" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public ImageVariant? Variant { get; set; }

    /// <summary>
    ///     Gets the <see cref="ViewContext" /> of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext? ViewContext { get; set; }

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
