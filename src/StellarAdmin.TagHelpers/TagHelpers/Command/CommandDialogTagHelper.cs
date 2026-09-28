using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A modal dialog that presents a command menu, typically as a command palette. Open and close
///     it like any other dialog, for example with an invoker button or <c>showModal()</c>.
/// </summary>
[HtmlTargetElement("sa-command-dialog")]
public class CommandDialogTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     The dialog's visually hidden description for assistive technology.
    /// </summary>
    /// <remarks>
    ///     Defaults to "Search for a command to run...".
    /// </remarks>
    [HtmlAttributeName("description")]
    public string? Description { get; set; }

    /// <summary>
    ///     Whether the dialog shows a close button in its corner.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>. Pressing Escape always closes the dialog.
    /// </remarks>
    [HtmlAttributeName("show-close-button")]
    public bool? ShowCloseButton { get; set; }

    /// <summary>
    ///     The dialog's visually hidden title for assistive technology.
    /// </summary>
    /// <remarks>
    ///     Defaults to "Command Palette".
    /// </remarks>
    [HtmlAttributeName("title")]
    public string? Title { get; set; }

    public CommandDialogTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveTitle = Title ?? "Command Palette";
        var effectiveDescription = Description ?? "Search for a command to run...";
        var dialogId =
            output.Attributes["id"]?.Value?.ToString()
            ?? $"sa-command-dialog-{GetUniqueId(context)}";
        var titleId = $"{dialogId}-title";
        var descriptionId = $"{dialogId}-description";

        output.TagName = "dialog";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("id", dialogId);
        output.Attributes.SetAttribute("aria-labelledby", titleId);
        output.Attributes.SetAttribute("aria-describedby", descriptionId);
        output.Attributes.SetAttribute("data-slot", "dialog-content");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-dialog-content", "sa-command-dialog", output.GetUserSuppliedClass())
        );

        // Wrap inside web component
        output.PreElement.AppendHtml("<sel-dialog>");
        output.PostElement.AppendHtml("</sel-dialog>");

        var header = new TagBuilder("div");
        header.Attributes["data-slot"] = "dialog-header";
        header.Attributes["class"] = JoinCssClasses("sa-dialog-header", "sr-only");

        var title = new TagBuilder("h2");
        title.Attributes["id"] = titleId;
        title.Attributes["data-slot"] = "dialog-title";
        title.Attributes["class"] = JoinCssClasses(
            "sa-dialog-title",
            "sa-font-heading",
            "font-heading"
        );
        title.InnerHtml.Append(effectiveTitle);

        var description = new TagBuilder("p");
        description.Attributes["id"] = descriptionId;
        description.Attributes["data-slot"] = "dialog-description";
        description.Attributes["class"] = "sa-dialog-description";
        description.InnerHtml.Append(effectiveDescription);

        header.InnerHtml.AppendHtml(title);
        header.InnerHtml.AppendHtml(description);
        output.PreContent.AppendHtml(header);

        if (ShowCloseButton == true)
        {
            var closeButtonOutput = new TagHelperOutput(
                "button",
                [
                    new TagHelperAttribute("type", "button"),
                    new TagHelperAttribute("class", "sa-dialog-close"),
                    new TagHelperAttribute("commandfor", dialogId),
                    new TagHelperAttribute("command", "close"),
                ],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );
            ButtonRenderingHelper.RenderAttributes(
                closeButtonOutput,
                ButtonVariant.Ghost,
                ButtonSize.IconSmall
            );
            closeButtonOutput.Content.AppendHtml(
                CommandRenderingHelper.RenderIcon(
                    context,
                    _iconOptions,
                    SemanticIconRole.Close,
                    "size-4"
                )
            );
            closeButtonOutput.Content.AppendHtml("<span class=\"sr-only\">Close</span>");

            output.PostContent.AppendHtml(closeButtonOutput);
        }

        return Task.CompletedTask;
    }
}
