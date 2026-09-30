using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The region that shows toast notifications. Place one in the layout, outside any region
///     that AJAX requests replace.
/// </summary>
/// <remarks>
///     Toasts added with <see cref="IToastNotifier" /> during a full page load, or before a
///     redirect to it, appear when the page loads. Classes set on the element apply to the
///     viewport that positions the toasts.
/// </remarks>
[HtmlTargetElement("sa-toaster", TagStructure = TagStructure.WithoutEndTag)]
public class ToasterTagHelper : StellarAdminTagHelperBase
{
    private static readonly (string Type, SemanticIconRole Role)[] TypeIcons =
    [
        ("success", SemanticIconRole.Success),
        ("info", SemanticIconRole.Info),
        ("warning", SemanticIconRole.Warning),
        ("error", SemanticIconRole.Error),
        ("loading", SemanticIconRole.Loading),
    ];

    private readonly IconOptions _iconOptions;

    public ToasterTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     How long a toast stays visible when it doesn't set its own duration.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>TimeSpan.FromSeconds(5)</c>. <c>TimeSpan.Zero</c> keeps toasts open
    ///     until they are closed.
    /// </remarks>
    [HtmlAttributeName("duration")]
    public TimeSpan? Duration { get; set; }

    /// <summary>
    ///     The maximum number of toasts visible at once. Older toasts are hidden until newer
    ///     ones close.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>3</c>.
    /// </remarks>
    [HtmlAttributeName("limit")]
    public int? Limit { get; set; }

    /// <summary>
    ///     Where on the screen toasts appear.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ToasterPosition.BottomRight" />.
    /// </remarks>
    [HtmlAttributeName("position")]
    public ToasterPosition? Position { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveDuration = Duration ?? TimeSpan.FromSeconds(5);
        var effectiveLimit = Limit ?? 3;
        var effectivePosition = Position ?? ToasterPosition.BottomRight;

        if (effectiveDuration < TimeSpan.Zero)
        {
            throw new InvalidOperationException("The duration of <sa-toaster> cannot be negative.");
        }

        if (effectiveLimit < 1)
        {
            throw new InvalidOperationException("The limit of <sa-toaster> must be at least 1.");
        }

        var userClass = output.GetUserSuppliedClass();
        output.Attributes.RemoveAll("class");

        output.TagName = "sel-toaster";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "toaster");
        output.Attributes.SetAttribute(
            "duration",
            ((long)effectiveDuration.TotalMilliseconds).ToString()
        );
        output.Attributes.SetAttribute("limit", effectiveLimit.ToString());

        output.Content.AppendHtml(await RenderToastTemplateAsync(context));
        output.Content.AppendHtml(RenderViewport(effectivePosition, userClass));

        // Toasts queued for this page, from this request or carried over a redirect. AJAX
        // responses have already moved theirs into the SA-Toasts header.
        var queued = ToastQueue.DrainAll(ViewContext.TempData);
        if (queued is not null)
        {
            var script = new TagBuilder("script");
            script.Attributes.Add("type", "application/json");
            script.InnerHtml.AppendHtml(queued);
            output.Content.AppendHtml(script);
        }
    }

    private static TagBuilder RenderViewport(ToasterPosition position, string? userClass)
    {
        // popover="manual" keeps the viewport in the top layer; sel-toaster shows it for the
        // page's lifetime so the live region exists before anything is announced.
        var viewport = new TagBuilder("div");
        viewport.Attributes.Add("data-slot", "toast-viewport");
        viewport.Attributes.Add("data-position", position.GetDataAttributeText());
        viewport.Attributes.Add("popover", "manual");
        viewport.Attributes.Add("tabindex", "-1");
        viewport.Attributes.Add("role", "region");
        viewport.Attributes.Add("aria-label", "Notifications");
        viewport.Attributes.Add("aria-live", "polite");
        viewport.Attributes.Add("aria-atomic", "false");
        viewport.Attributes.Add("aria-relevant", "additions text");
        viewport.Attributes.Add("class", JoinCssClasses("sa-toast-viewport", userClass));

        // Error toasts are also announced assertively. The announcer lives inside the viewport
        // so it moves into a modal dialog along with the toasts.
        var announcer = new TagBuilder("div");
        announcer.Attributes.Add("data-slot", "toast-announcer");
        announcer.Attributes.Add("aria-live", "assertive");
        announcer.Attributes.Add("class", "sr-only");
        viewport.InnerHtml.AppendHtml(announcer);

        return viewport;
    }

    private async Task<IHtmlContent> RenderToastTemplateAsync(TagHelperContext context)
    {
        var icon = new TagBuilder("span");
        icon.Attributes.Add("data-slot", "toast-icon");
        icon.Attributes.Add("class", "sa-toast-icon");
        foreach (var (type, role) in TypeIcons)
        {
            icon.InnerHtml.AppendHtml(
                await RenderIconAsync(
                    context,
                    role,
                    new TagHelperAttribute("data-toast-icon", type)
                )
            );
        }

        var title = new TagBuilder("div");
        title.Attributes.Add("data-slot", "toast-title");
        title.Attributes.Add("class", "sa-toast-title");

        var description = new TagBuilder("div");
        description.Attributes.Add("data-slot", "toast-description");
        description.Attributes.Add("class", "sa-toast-description");

        var text = new TagBuilder("div");
        text.Attributes.Add("data-slot", "toast-text");
        text.Attributes.Add("class", "sa-toast-text");
        text.InnerHtml.AppendHtml(title);
        text.InnerHtml.AppendHtml(description);

        var action = new TagHelperOutput(
            "a",
            [
                new TagHelperAttribute("data-slot", "toast-action"),
                new TagHelperAttribute("class", "sa-toast-action"),
            ],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        ButtonRenderingHelper.RenderAttributes(action, ButtonVariant.Outline, ButtonSize.Small);

        var close = new TagHelperOutput(
            "button",
            [
                new TagHelperAttribute("type", "button"),
                new TagHelperAttribute("data-slot", "toast-close"),
                new TagHelperAttribute("aria-label", "Close toast"),
                new TagHelperAttribute("class", "sa-toast-close"),
            ],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        ButtonRenderingHelper.RenderAttributes(close, ButtonVariant.Ghost, ButtonSize.IconSmall);
        close.Content.AppendHtml(await RenderIconAsync(context, SemanticIconRole.Close));

        var content = new TagBuilder("div");
        content.Attributes.Add("data-slot", "toast-content");
        content.Attributes.Add("class", "sa-toast-content");
        content.InnerHtml.AppendHtml(icon);
        content.InnerHtml.AppendHtml(text);
        content.InnerHtml.AppendHtml(action);
        content.InnerHtml.AppendHtml(close);

        var toast = new TagBuilder("div");
        toast.Attributes.Add("data-slot", "toast");
        toast.Attributes.Add("tabindex", "0");
        toast.Attributes.Add("role", "dialog");
        toast.Attributes.Add("aria-modal", "false");
        toast.Attributes.Add("class", "sa-toast");
        toast.InnerHtml.AppendHtml(content);

        // sel-toaster clones this for every toast, whether it came from the server or the browser.
        var template = new TagBuilder("template");
        template.Attributes.Add("data-slot", "toast-template");
        template.InnerHtml.AppendHtml(toast);

        return template;
    }

    private async Task<TagHelperOutput> RenderIconAsync(
        TagHelperContext context,
        SemanticIconRole role,
        params TagHelperAttribute[] attributes
    )
    {
        var iconOutput = new TagHelperOutput(
            "svg",
            [.. attributes, new TagHelperAttribute("aria-hidden", "true")],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var iconTagHelper = new IconTagHelper(_iconOptions)
        {
            Name = _iconOptions.GetSemanticIconName(role),
        };
        await iconTagHelper.ProcessAsync(context, iconOutput);

        return iconOutput;
    }
}
