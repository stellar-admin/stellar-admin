using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

internal sealed record ImageAppearance(
    ImageVariant Variant,
    ImageSize Size,
    ImageAspectRatio AspectRatio,
    ImageFit Fit,
    ImageInteraction Interaction
);

internal static class ImageRendering
{
    private const string LightboxRenderedKey = "StellarAdmin.UI.ImageLightbox";

    public static ImageAppearance Thumbnail(ImageSize size) =>
        new(
            ImageVariant.Thumbnail,
            size,
            ImageAspectRatio.Square,
            ImageFit.Cover,
            ImageInteraction.None
        );

    // The image box: an <img> when there is a source, and the placeholder and error fallbacks,
    // which the stylesheet shows for the empty and error states. A lightbox image is a button.
    public static async Task<TagBuilder> RenderImageAsync(
        TagHelperContext context,
        IconOptions iconOptions,
        string? source,
        string? alt,
        ImageAppearance appearance
    )
    {
        var hasSource = !string.IsNullOrEmpty(source);
        var isButton = hasSource && appearance.Interaction == ImageInteraction.Lightbox;

        var image = new TagBuilder(isButton ? "button" : "span");
        image.Attributes.Add("data-slot", "image");
        image.Attributes.Add("data-variant", appearance.Variant.GetDataAttributeText());
        image.Attributes.Add("data-size", appearance.Size.GetDataAttributeText());
        image.Attributes.Add("data-aspect-ratio", appearance.AspectRatio.GetDataAttributeText());
        image.Attributes.Add("data-fit", appearance.Fit.GetDataAttributeText());
        image.Attributes.Add("data-state", hasSource ? "loading" : "empty");
        if (hasSource && appearance.Interaction != ImageInteraction.None)
        {
            image.Attributes.Add("data-interaction", appearance.Interaction.GetDataAttributeText());
        }

        image.Attributes.Add("class", "sa-image");

        if (isButton)
        {
            image.Attributes.Add("type", "button");
            image.Attributes.Add("aria-haspopup", "dialog");
            image.Attributes.Add("aria-label", string.IsNullOrEmpty(alt) ? "View image" : alt);
        }
        else if (!hasSource)
        {
            image.Attributes.Add("role", "img");
            image.Attributes.Add("aria-label", "No image");
        }

        if (hasSource)
        {
            var img = new TagBuilder("img") { TagRenderMode = TagRenderMode.SelfClosing };
            img.Attributes.Add("src", source);
            img.Attributes.Add("alt", isButton ? string.Empty : alt ?? string.Empty);
            img.Attributes.Add("decoding", "async");
            image.InnerHtml.AppendHtml(img);
        }

        var isFigure = appearance.Variant == ImageVariant.Figure;
        image.InnerHtml.AppendHtml(
            await RenderFallbackAsync(
                context,
                iconOptions,
                "image-placeholder",
                SemanticIconRole.Image,
                isFigure ? "No image" : null
            )
        );
        image.InnerHtml.AppendHtml(
            await RenderFallbackAsync(
                context,
                iconOptions,
                "image-error",
                SemanticIconRole.ImageError,
                isFigure ? "Couldn't load the image" : null
            )
        );

        return image;
    }

    public static async Task<IHtmlContent> RenderIconAsync(
        TagHelperContext context,
        IconOptions iconOptions,
        SemanticIconRole role
    )
    {
        var iconOutput = new TagHelperOutput(
            "svg",
            [new TagHelperAttribute("aria-hidden", "true")],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var iconTagHelper = new IconTagHelper(iconOptions)
        {
            Name = iconOptions.GetSemanticIconName(role),
        };
        await iconTagHelper.ProcessAsync(context, iconOutput);

        return iconOutput;
    }

    // The lightbox dialog the script opens, as a template it clones once. It is written with the
    // first image of a response that can open it, so pages and partial responses both carry it.
    public static async Task<IHtmlContent?> RenderLightboxTemplateOnceAsync(
        TagHelperContext context,
        IconOptions iconOptions,
        HttpContext? httpContext
    )
    {
        if (httpContext is not null)
        {
            if (httpContext.Items.ContainsKey(LightboxRenderedKey))
            {
                return null;
            }

            httpContext.Items[LightboxRenderedKey] = true;
        }

        var close = new TagHelperOutput(
            "button",
            [
                new TagHelperAttribute("type", "button"),
                new TagHelperAttribute("commandfor", "sa-image-lightbox"),
                new TagHelperAttribute("command", "close"),
                new TagHelperAttribute("aria-label", "Close"),
                new TagHelperAttribute("class", "sa-image-lightbox-close"),
            ],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        ButtonRenderingHelper.RenderAttributes(
            close,
            ButtonVariant.Secondary,
            ButtonSize.IconSmall
        );
        close.Content.AppendHtml(
            await RenderIconAsync(context, iconOptions, SemanticIconRole.Close)
        );

        var img = new TagBuilder("img") { TagRenderMode = TagRenderMode.SelfClosing };
        img.Attributes.Add("data-slot", "image-lightbox-image");
        img.Attributes.Add("alt", string.Empty);

        var dialog = new TagBuilder("dialog");
        dialog.Attributes.Add("id", "sa-image-lightbox");
        dialog.Attributes.Add("data-slot", "dialog-content");
        dialog.Attributes.Add("class", "sa-dialog-content sa-image-lightbox");
        dialog.Attributes.Add("closedby", "any");
        dialog.Attributes.Add("aria-label", "Image");
        dialog.InnerHtml.AppendHtml(img);
        dialog.InnerHtml.AppendHtml(close);

        var host = new TagBuilder("sel-dialog");
        host.InnerHtml.AppendHtml(dialog);

        var template = new TagBuilder("template");
        template.Attributes.Add("data-slot", "image-lightbox-template");
        template.InnerHtml.AppendHtml(host);

        return template;
    }

    private static async Task<TagBuilder> RenderFallbackAsync(
        TagHelperContext context,
        IconOptions iconOptions,
        string slot,
        SemanticIconRole role,
        string? text
    )
    {
        var fallback = new TagBuilder("span");
        fallback.Attributes.Add("data-slot", slot);
        fallback.Attributes.Add("class", "sa-image-fallback");
        fallback.InnerHtml.AppendHtml(await RenderIconAsync(context, iconOptions, role));
        if (text is not null)
        {
            var label = new TagBuilder("span");
            label.InnerHtml.Append(text);
            fallback.InnerHtml.AppendHtml(label);
        }

        return fallback;
    }
}
