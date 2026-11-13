using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an image picker that uploads the picked file straight away.
/// </summary>
/// <remarks>
///     <para>
///         The file is posted to <see cref="UploadUrl" /> as the multipart field <c>file</c>, with
///         the form's antiforgery token. The endpoint answers with JSON:
///         <c>{ "value": "...", "url": "..." }</c> on success, or <c>{ "error": "..." }</c> with an
///         error status. The form posts the returned value.
///     </para>
///     <para>
///         Replacing and removing the image take effect when the form is saved. Requires
///         <c>stellar-admin.js</c>.
///     </para>
/// </remarks>
[HtmlTargetElement("sa-image-upload", TagStructure = TagStructure.WithoutEndTag)]
public sealed class ImageUploadTagHelper : FieldInputBaseTagHelper<ImageUploadClassNames>
{
    private const string DefaultAccept = "image/jpeg,image/png,image/webp,image/gif,image/avif";
    private const long DefaultMaxFileSize = 5 * 1024 * 1024;

    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ImageUploadTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders a bound label and error.</param>
    /// <param name="iconOptions">The icon options that supply the control's icons.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public ImageUploadTagHelper(IHtmlGenerator htmlGenerator, IOptions<IconOptions> iconOptions)
        : base(htmlGenerator)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Gets or sets the accepted image types.
    /// </summary>
    /// <value>
    ///     A comma-separated list of media types. The default is JPEG, PNG, WebP, GIF and AVIF.
    /// </value>
    [HtmlAttributeName("accept")]
    public string? Accept { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the control is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the control is disabled; otherwise, <see langword="false" />.
    ///     The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     A disabled control doesn't post its value.
    /// </remarks>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets the <c>id</c> of the form that the value belongs to.
    /// </summary>
    /// <remarks>
    ///     Use it when the control is outside that form.
    /// </remarks>
    [HtmlAttributeName("form")]
    public string? FormName { get; set; }

    /// <summary>
    ///     Gets or sets how the control arranges the image and its actions.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the layout. The default is
    ///     <see cref="ImageUploadLayout.Dropzone" />.
    /// </value>
    [HtmlAttributeName("layout")]
    public ImageUploadLayout? Layout { get; set; }

    /// <summary>
    ///     Gets or sets the largest file accepted.
    /// </summary>
    /// <value>
    ///     The size in bytes. The default is <c>5242880</c> (5 MB).
    /// </value>
    /// <remarks>
    ///     The browser checks the size before uploading. The upload endpoint must check it again.
    /// </remarks>
    [HtmlAttributeName("max-file-size")]
    public long? MaxFileSize { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the image can't be changed.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the image can't be changed; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     A read-only control shows the image alone, without the upload area, the buttons or the
    ///     accepted types, and its value is posted.
    /// </remarks>
    [HtmlAttributeName("readonly")]
    public bool? ReadOnly { get; set; }

    /// <summary>
    ///     Gets or sets the URL of the current image.
    /// </summary>
    /// <value>
    ///     The default is the stored value, used as a URL.
    /// </value>
    [HtmlAttributeName("src")]
    public string? Source { get; set; }

    /// <summary>
    ///     Gets or sets the URL that a picked file is posted to.
    /// </summary>
    /// <remarks>
    ///     Required unless the control is disabled or read-only.
    /// </remarks>
    [HtmlAttributeName("upload-url")]
    public string? UploadUrl { get; set; }

    /// <summary>
    ///     Gets or sets the stored value.
    /// </summary>
    /// <remarks>
    ///     Used when the control isn't bound with <c>asp-for</c>.
    /// </remarks>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     <see cref="UploadUrl" /> is empty and the control isn't disabled.
    /// </exception>
    protected override Task<FieldLayout> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    )
    {
        var effectiveLayout = Layout ?? ImageUploadLayout.Dropzone;
        var effectiveDisabled = Disabled ?? false;
        var effectiveReadOnly = !effectiveDisabled && (ReadOnly ?? false);
        var effectiveMaxFileSize = MaxFileSize ?? DefaultMaxFileSize;
        var accept = string.IsNullOrWhiteSpace(Accept) ? DefaultAccept : Accept;
        if (string.IsNullOrEmpty(UploadUrl) && !effectiveDisabled && !effectiveReadOnly)
        {
            throw new InvalidOperationException(
                "An image upload needs an upload-url unless it is disabled or read-only."
            );
        }

        var value = For?.Model?.ToString() ?? Value;
        var hasValue = !string.IsNullOrEmpty(value);
        var source = string.IsNullOrEmpty(Source) ? value : Source;
        var state = hasValue ? "filled" : "empty";
        var acceptText = DescribeTypes(accept);

        output.Attributes.RemoveAll("name");
        output.TagName = "sel-image-upload";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("data-slot", "image-upload");
        output.Attributes.SetAttribute("data-layout", effectiveLayout.GetDataAttributeText());
        output.Attributes.SetAttribute("data-state", state);
        output.Attributes.SetAttribute("role", "group");
        output.Attributes.SetAttribute("data-upload-url", UploadUrl ?? string.Empty);
        output.Attributes.SetAttribute(
            "data-max-file-size",
            effectiveMaxFileSize.ToString(CultureInfo.InvariantCulture)
        );
        output.Attributes.SetAttribute("data-accept", accept);
        output.Attributes.SetAttribute("data-accept-text", acceptText);
        if (hasValue)
        {
            output.Attributes.SetAttribute("data-file-name", FileName(value!));
        }

        if (effectiveDisabled)
        {
            output.Attributes.SetAttribute("data-disabled", "true");
        }

        if (effectiveReadOnly)
        {
            output.Attributes.SetAttribute("data-readonly", "true");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-image-upload", ClassNames?.Control, output.GetUserSuppliedClass())
        );

        ApplyFieldAttributes(context, output.Attributes, FieldLayout.Stacked);
        if (WillRenderFieldLabel())
        {
            LabelId = $"sa-{GetUniqueId(context)}-label";
            if (!output.Attributes.ContainsName("aria-labelledby"))
            {
                output.Attributes.SetAttribute("aria-labelledby", LabelId);
            }
        }

        var file = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
        file.Attributes.Add("type", "file");
        file.Attributes.Add("data-slot", "image-upload-file");
        file.Attributes.Add("class", "sa-image-upload-file");
        file.Attributes.Add("accept", accept);
        file.Attributes.Add("tabindex", "-1");
        file.Attributes.Add("aria-hidden", "true");
        if (effectiveDisabled || effectiveReadOnly)
        {
            file.Attributes.Add("disabled", "disabled");
        }

        output.Content.AppendHtml(file);

        var hidden = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
        hidden.Attributes.Add("type", "hidden");
        hidden.Attributes.Add("data-slot", "image-upload-value");
        if (ResolveName() is { } name)
        {
            hidden.Attributes.Add("name", name);
        }

        hidden.Attributes.Add("value", value ?? string.Empty);
        if (FormName != null)
        {
            hidden.Attributes.Add("form", FormName);
        }

        if (effectiveDisabled)
        {
            hidden.Attributes.Add("disabled", "disabled");
        }

        output.Content.AppendHtml(hidden);

        var parts = new Parts(
            _iconOptions,
            state,
            effectiveDisabled,
            effectiveReadOnly,
            ClassNames?.Image
        );
        switch (effectiveLayout)
        {
            case ImageUploadLayout.Compact:
                RenderCompact(parts, output.Content, source);
                break;
            case ImageUploadLayout.Attachment:
                RenderAttachment(parts, output.Content, source, value);
                break;
            case ImageUploadLayout.Cell:
                RenderCell(parts, output.Content, source);
                break;
            default:
                RenderDropzone(parts, output.Content, source);
                break;
        }

        // Nothing can be uploaded to a read-only control, so it has no hint or error
        if (effectiveLayout == ImageUploadLayout.Cell && !effectiveReadOnly)
        {
            var error = new TagBuilder("span");
            error.Attributes.Add("data-slot", "image-upload-note");
            error.Attributes.Add("class", JoinCssClasses("sa-cell-error", ClassNames?.Note));
            error.Attributes.Add("role", "alert");
            error.Attributes.Add("hidden", "hidden");
            output.Content.AppendHtml(error);
        }
        else if (!effectiveReadOnly)
        {
            var note = new TagBuilder("p");
            note.Attributes.Add("data-slot", "image-upload-note");
            note.Attributes.Add("class", JoinCssClasses("sa-image-upload-note", ClassNames?.Note));
            note.Attributes.Add("aria-live", "polite");
            note.Attributes.Add(
                "data-hint",
                $"{acceptText}, up to {FormatSize(effectiveMaxFileSize)}."
            );
            note.InnerHtml.Append($"{acceptText}, up to {FormatSize(effectiveMaxFileSize)}.");
            if (effectiveLayout == ImageUploadLayout.Attachment && hasValue)
            {
                note.Attributes.Add("hidden", "hidden");
            }

            output.Content.AppendHtml(note);
        }

        if (!effectiveDisabled && effectiveLayout != ImageUploadLayout.Cell)
        {
            output.PostElement.AppendHtml(
                ImageRendering.RenderLightboxTemplateOnce(_iconOptions, ViewContext.HttpContext)
            );
        }

        return Task.FromResult(FieldLayout.Stacked);
    }

    private static void RenderDropzone(Parts parts, TagHelperContent content, string? source)
    {
        // Read-only shows the image, or the empty image, alone
        if (parts.ReadOnly)
        {
            var image = parts.Media(null);
            image.InnerHtml.AppendHtml(
                parts.Image(
                    source,
                    new ImageAppearance(
                        ImageVariant.Figure,
                        ImageSize.Medium,
                        ImageAspectRatio.Original,
                        ImageFit.Cover,
                        ImageInteraction.None
                    )
                )
            );
            content.AppendHtml(image);
            return;
        }

        content.AppendHtml(parts.Drop(row: false));

        var media = parts.Media("filled uploading removed");
        media.InnerHtml.AppendHtml(
            parts.Image(
                source,
                new ImageAppearance(
                    ImageVariant.Figure,
                    ImageSize.Medium,
                    ImageAspectRatio.Original,
                    ImageFit.Cover,
                    ImageInteraction.None
                )
            )
        );
        media.InnerHtml.AppendHtml(parts.Progress(bar: true));

        var overlay = new TagBuilder("div");
        overlay.Attributes.Add("class", "sa-image-upload-overlay");
        overlay.InnerHtml.AppendHtml(
            parts.Button(
                "pick",
                "filled",
                ButtonVariant.Secondary,
                ButtonSize.Small,
                "Replace",
                SemanticIconRole.Upload
            )
        );
        overlay.InnerHtml.AppendHtml(
            parts.Button(
                "remove",
                "filled",
                ButtonVariant.Secondary,
                ButtonSize.IconSmall,
                null,
                SemanticIconRole.Remove,
                "Remove image"
            )
        );
        overlay.InnerHtml.AppendHtml(
            parts.Button(
                "cancel",
                "uploading",
                ButtonVariant.Secondary,
                ButtonSize.Small,
                "Cancel",
                SemanticIconRole.Close
            )
        );
        media.InnerHtml.AppendHtml(overlay);
        content.AppendHtml(media);
    }

    private static void RenderCompact(Parts parts, TagHelperContent content, string? source)
    {
        var media = parts.Media(null);
        media.InnerHtml.AppendHtml(parts.Image(source, ImageRendering.Thumbnail(ImageSize.Large)));
        media.InnerHtml.AppendHtml(parts.Progress(bar: false));
        content.AppendHtml(media);
        if (parts.ReadOnly)
        {
            return;
        }

        var actions = new TagBuilder("div");
        actions.Attributes.Add("class", "sa-image-upload-actions");
        actions.InnerHtml.AppendHtml(
            parts.Button(
                "pick",
                "empty removed",
                ButtonVariant.Outline,
                ButtonSize.Small,
                "Upload",
                SemanticIconRole.Upload
            )
        );
        actions.InnerHtml.AppendHtml(
            parts.Button(
                "pick",
                "filled",
                ButtonVariant.Outline,
                ButtonSize.Small,
                "Replace",
                SemanticIconRole.Upload
            )
        );
        actions.InnerHtml.AppendHtml(
            parts.Button("remove", "filled", ButtonVariant.Ghost, ButtonSize.Small, "Remove", null)
        );
        actions.InnerHtml.AppendHtml(
            parts.Button(
                "cancel",
                "uploading",
                ButtonVariant.Outline,
                ButtonSize.Small,
                "Cancel",
                null
            )
        );
        content.AppendHtml(actions);
    }

    private static void RenderAttachment(
        Parts parts,
        TagHelperContent content,
        string? source,
        string? value
    )
    {
        if (!parts.ReadOnly)
        {
            content.AppendHtml(parts.Drop(row: true));
        }

        var attachment = new TagBuilder("div");
        attachment.Attributes.Add("data-slot", "attachment");
        attachment.Attributes.Add("data-state", "done");
        attachment.Attributes.Add("data-size", "default");
        attachment.Attributes.Add("data-orientation", "horizontal");
        attachment.Attributes.Add(
            "class",
            "sa-attachment sa-attachment-size-default sa-attachment-orientation-horizontal"
        );
        if (!parts.ReadOnly)
        {
            parts.ShowWhen(attachment, "filled uploading removed");
        }

        var media = new TagBuilder("div");
        media.Attributes.Add("data-slot", "attachment-media");
        media.Attributes.Add("data-variant", "image");
        media.Attributes.Add("class", "sa-attachment-media sa-attachment-media-variant-image");
        media.InnerHtml.AppendHtml(parts.Image(source, ImageRendering.Thumbnail(ImageSize.Medium)));
        media.InnerHtml.AppendHtml(parts.Progress(bar: false));
        attachment.InnerHtml.AppendHtml(media);

        var title = new TagBuilder("span");
        title.Attributes.Add("data-slot", "attachment-title");
        title.Attributes.Add("class", "sa-attachment-title");
        title.InnerHtml.Append(string.IsNullOrEmpty(value) ? string.Empty : FileName(value));

        var description = new TagBuilder("span");
        description.Attributes.Add("data-slot", "attachment-description");
        description.Attributes.Add("class", "sa-attachment-description");
        description.Attributes.Add("hidden", "hidden");

        var attachmentContent = new TagBuilder("div");
        attachmentContent.Attributes.Add("data-slot", "attachment-content");
        attachmentContent.Attributes.Add("class", "sa-attachment-content");
        attachmentContent.InnerHtml.AppendHtml(title);
        attachmentContent.InnerHtml.AppendHtml(description);
        attachment.InnerHtml.AppendHtml(attachmentContent);
        if (parts.ReadOnly)
        {
            content.AppendHtml(attachment);
            return;
        }

        var actions = new TagBuilder("div");
        actions.Attributes.Add("data-slot", "attachment-actions");
        actions.Attributes.Add("class", "sa-attachment-actions");
        actions.InnerHtml.AppendHtml(
            parts.Button(
                "pick",
                "filled",
                ButtonVariant.Ghost,
                ButtonSize.IconExtraSmall,
                null,
                SemanticIconRole.Upload,
                "Replace image"
            )
        );
        actions.InnerHtml.AppendHtml(
            parts.Button(
                "remove",
                "filled",
                ButtonVariant.Ghost,
                ButtonSize.IconExtraSmall,
                null,
                SemanticIconRole.Remove,
                "Remove image"
            )
        );
        actions.InnerHtml.AppendHtml(
            parts.Button(
                "cancel",
                "uploading",
                ButtonVariant.Ghost,
                ButtonSize.IconExtraSmall,
                null,
                SemanticIconRole.Close,
                "Cancel upload"
            )
        );
        actions.InnerHtml.AppendHtml(
            parts.Button(
                "undo",
                "removed",
                ButtonVariant.Ghost,
                ButtonSize.ExtraSmall,
                "Undo",
                null
            )
        );
        attachment.InnerHtml.AppendHtml(actions);
        content.AppendHtml(attachment);
    }

    private static void RenderCell(Parts parts, TagHelperContent content, string? source)
    {
        if (parts.ReadOnly)
        {
            content.AppendHtml(parts.Image(source, ImageRendering.Thumbnail(ImageSize.Small)));
            return;
        }

        var trigger = new TagBuilder("button");
        trigger.Attributes.Add("type", "button");
        trigger.Attributes.Add("data-slot", "image-upload-trigger");
        trigger.Attributes.Add("data-action", "pick");
        trigger.Attributes.Add("class", "sa-image-upload-trigger");
        trigger.Attributes.Add(
            "aria-label",
            string.IsNullOrEmpty(source) ? "Upload image" : "Replace image"
        );
        if (parts.Disabled)
        {
            trigger.Attributes.Add("disabled", "disabled");
        }

        trigger.InnerHtml.AppendHtml(
            parts.Image(source, ImageRendering.Thumbnail(ImageSize.Small))
        );
        var hover = new TagBuilder("span");
        hover.Attributes.Add("class", "sa-image-upload-trigger-icon");
        hover.InnerHtml.AppendHtml(
            parts.IconOptions.RenderIcon(SemanticIconRole.Upload, ("aria-hidden", "true"))
        );
        trigger.InnerHtml.AppendHtml(hover);
        trigger.InnerHtml.AppendHtml(parts.Progress(bar: false));
        content.AppendHtml(trigger);

        var clear = parts.Button(
            "remove",
            "filled",
            ButtonVariant.Secondary,
            ButtonSize.IconExtraSmall,
            null,
            SemanticIconRole.Close,
            "Remove image"
        );
        clear.Attributes.SetAttribute(
            "class",
            JoinCssClasses(clear.Attributes["class"].Value?.ToString(), "sa-image-upload-clear")
        );
        content.AppendHtml(clear);
    }

    // "PNG, JPEG, WebP, GIF or AVIF" for the hint and the type error.
    private static string DescribeTypes(string accept)
    {
        var names = accept
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(type =>
                type.ToLowerInvariant() switch
                {
                    "image/jpeg" or "image/jpg" => "JPEG",
                    "image/png" => "PNG",
                    "image/webp" => "WebP",
                    "image/gif" => "GIF",
                    "image/avif" => "AVIF",
                    "image/svg+xml" => "SVG",
                    "image/*" => "any image",
                    var other => other[(other.IndexOf('/') + 1)..].ToUpperInvariant(),
                }
            )
            .Distinct()
            .ToList();

        return names.Count switch
        {
            0 => "Images",
            1 => names[0],
            _ => $"{string.Join(", ", names[..^1])} or {names[^1]}",
        };
    }

    private static string FileName(string value)
    {
        var path = value.Split('?', '#')[0].TrimEnd('/', '\\');
        var start = path.LastIndexOfAny(['/', '\\']) + 1;

        return path[start..];
    }

    private static string FormatSize(long bytes) =>
        bytes >= 1024 * 1024
            ? $"{Math.Round(bytes / 1024d / 1024d, 1).ToString(CultureInfo.InvariantCulture)} MB"
            : $"{Math.Max(1, Math.Round(bytes / 1024d)).ToString(CultureInfo.InvariantCulture)} KB";

    private string? ResolveName()
    {
        if (!string.IsNullOrEmpty(Name))
        {
            return Name;
        }

        // An editor template binds asp-for="@Model", whose name is empty and lives in the prefix
        if (For != null)
        {
            var fullName = ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(For.Name);

            return string.IsNullOrEmpty(fullName) ? null : fullName;
        }

        return null;
    }

    // Builds the layouts' parts. Each part that only belongs to some states names them in
    // data-when; the script shows and hides the parts as the state changes.
    private sealed class Parts(
        IconOptions iconOptions,
        string state,
        bool disabled,
        bool readOnly,
        string? imageClass
    )
    {
        public bool Disabled { get; } = disabled;

        public IconOptions IconOptions { get; } = iconOptions;

        public bool ReadOnly { get; } = readOnly;

        public TagHelperOutput Button(
            string action,
            string when,
            ButtonVariant variant,
            ButtonSize size,
            string? text,
            SemanticIconRole? icon,
            string? ariaLabel = null
        )
        {
            var button = new TagHelperOutput(
                "button",
                [
                    new TagHelperAttribute("type", "button"),
                    new TagHelperAttribute("data-action", action),
                ],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );
            ButtonRenderingHelper.RenderAttributes(button, variant, size);
            button.Attributes.SetAttribute("data-when", when);
            if (!when.Split(' ').Contains(state))
            {
                button.Attributes.SetAttribute("hidden", "hidden");
            }

            if (ariaLabel != null)
            {
                button.Attributes.SetAttribute("aria-label", ariaLabel);
            }

            if (Disabled)
            {
                button.Attributes.SetAttribute("disabled", "disabled");
            }

            if (icon is { } role)
            {
                var iconOutput = IconOptions.RenderIcon(role, ("aria-hidden", "true"));
                if (text != null)
                {
                    iconOutput.Attributes.SetAttribute("data-icon", "inline-start");
                }

                button.Content.AppendHtml(iconOutput);
            }

            if (text != null)
            {
                button.Content.Append(text);
            }

            return button;
        }

        public TagBuilder Drop(bool row)
        {
            var drop = new TagBuilder("div");
            drop.Attributes.Add("data-slot", "image-upload-drop");
            drop.Attributes.Add("data-action", "pick");
            drop.Attributes.Add("role", "button");
            drop.Attributes.Add("tabindex", Disabled ? "-1" : "0");
            if (Disabled)
            {
                drop.Attributes.Add("aria-disabled", "true");
            }

            if (row)
            {
                drop.Attributes.Add("data-size", "row");
            }

            drop.Attributes.Add("class", "sa-image-upload-drop");
            ShowWhen(drop, "empty");

            var icon = new TagBuilder("span");
            icon.Attributes.Add("class", "sa-image-upload-drop-icon");
            icon.InnerHtml.AppendHtml(
                IconOptions.RenderIcon(SemanticIconRole.Upload, ("aria-hidden", "true"))
            );
            drop.InnerHtml.AppendHtml(icon);

            var strong = new TagBuilder("strong");
            strong.InnerHtml.Append("Click to upload");
            var text = new TagBuilder("span");
            text.InnerHtml.AppendHtml(strong);
            text.InnerHtml.Append(" or drag and drop");
            drop.InnerHtml.AppendHtml(text);

            return drop;
        }

        public TagBuilder Image(string? source, ImageAppearance appearance)
        {
            var image = ImageRendering.RenderImage(IconOptions, source, null, appearance);
            if (imageClass is not null)
            {
                image.Attributes["class"] = JoinCssClasses(image.Attributes["class"], imageClass);
            }

            return image;
        }

        public TagBuilder Media(string? when)
        {
            var media = new TagBuilder("div");
            media.Attributes.Add("data-slot", "image-upload-media");
            media.Attributes.Add("class", "sa-image-upload-media");
            if (when != null)
            {
                ShowWhen(media, when);
            }

            return media;
        }

        public IHtmlContent Progress(bool bar)
        {
            var progress = new TagBuilder("span");
            progress.Attributes.Add("data-slot", "image-upload-progress");
            progress.Attributes.Add("class", "sa-image-upload-progress");
            ShowWhen(progress, "uploading");

            var spinner = IconOptions.RenderIcon(SemanticIconRole.Loading, ("aria-hidden", "true"));
            spinner.Attributes.SetAttribute("class", "sa-spinner");
            spinner.Attributes.SetAttribute("role", "status");
            spinner.Attributes.SetAttribute("aria-label", "Uploading");
            spinner.Attributes.RemoveAll("aria-hidden");
            progress.InnerHtml.AppendHtml(spinner);

            if (bar)
            {
                var track = new TagBuilder("span");
                track.Attributes.Add("class", "sa-image-upload-bar");
                var fill = new TagBuilder("span");
                fill.Attributes.Add("data-slot", "image-upload-bar");
                track.InnerHtml.AppendHtml(fill);
                progress.InnerHtml.AppendHtml(track);
            }

            return progress;
        }

        public void ShowWhen(TagBuilder element, string when)
        {
            element.Attributes.Add("data-when", when);
            if (!when.Split(' ').Contains(state))
            {
                element.Attributes.Add("hidden", "hidden");
            }
        }
    }
}
