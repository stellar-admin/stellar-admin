using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a panel that slides in from an edge of the screen.
/// </summary>
/// <remarks>
///     The sheet is a native <c>&lt;dialog&gt;</c> element. Open it with a button that has
///     <c>commandfor</c> set to the sheet's <c>id</c> and <c>command="show-modal"</c>. Close it
///     with <c>command="close"</c>.
/// </remarks>
[HtmlTargetElement("sa-sheet")]
public class SheetTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SheetTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the close button's icon.</param>
    public SheetTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Gets or sets a value that indicates whether the sheet shows a close button in its corner.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the sheet shows a close button; otherwise,
    ///     <see langword="false" />. The default is <see langword="true" />.
    /// </value>
    [HtmlAttributeName("show-close-button")]
    public bool? ShowCloseButton { get; set; }

    /// <summary>
    ///     Gets or sets the edge of the screen that the sheet slides in from.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the edge. The default is
    ///     <see cref="SheetSide.Right" />.
    /// </value>
    [HtmlAttributeName("side")]
    public SheetSide? Side { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveShowCloseButton = ShowCloseButton ?? true;
        var effectiveSide = Side ?? SheetSide.Right;

        output.TagName = "dialog";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sheet-content");
        output.Attributes.SetAttribute("data-side", effectiveSide.GetDataAttributeText());
        // Structural styling (including the per-side placement, keyed off data-side) lives in
        // Client/css/components/sheet.css.
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sheet-content", output.GetUserSuppliedClass())
        );

        // Wrap inside web component
        output.PreElement.AppendHtml("<sel-dialog>");
        output.PostElement.AppendHtml("</sel-dialog>");

        // Add the close button
        if (effectiveShowCloseButton)
        {
            var id = output.Attributes.TryGetAttribute("id", out var idAttribute)
                ? idAttribute.Value.ToString()
                : null;

            if (id == null)
            {
                id = $"--sa-sheet-{GetUniqueId(context)}";
                output.Attributes.SetAttribute("id", id);
            }

            // Render the icon
            var iconOutput = new TagHelperOutput(
                "svg",
                [new TagHelperAttribute("class", "size-4")],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );
            var iconTagHelper = new IconTagHelper(_iconOptions)
            {
                Name = _iconOptions.GetSemanticIconName(SemanticIconRole.Close),
            };
            await iconTagHelper.ProcessAsync(context, iconOutput);

            // Render the button. Icon-only, so it carries a visually hidden accessible name.
            var closeButtonOutput = new TagHelperOutput(
                "button",
                [
                    new TagHelperAttribute("type", "button"),
                    new TagHelperAttribute("class", "sa-sheet-close"),
                    new TagHelperAttribute("commandfor", id),
                    new TagHelperAttribute("command", "close"),
                ],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );
            ButtonRenderingHelper.RenderAttributes(
                closeButtonOutput,
                ButtonVariant.Ghost,
                ButtonSize.IconSmall
            );
            closeButtonOutput.Content.AppendHtml(iconOutput);
            closeButtonOutput.Content.AppendHtml("<span class=\"sr-only\">Close</span>");

            output.Content.AppendHtml(closeButtonOutput);
        }

        // Add content wrapper
        var contentTagBuilder = new TagBuilder("div");
        contentTagBuilder.Attributes.Add("class", "sa-sheet-body");
        contentTagBuilder.InnerHtml.AppendHtml(await output.GetChildContentAsync());
        output.Content.AppendHtml(contentTagBuilder);
    }
}
