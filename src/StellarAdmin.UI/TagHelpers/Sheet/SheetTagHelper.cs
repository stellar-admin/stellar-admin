using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A panel that slides in from an edge of the screen, rendered over a native
///     <c>&lt;dialog&gt;</c> element. Open and close it with the Invoker Commands API — a
///     trigger button carrying <c>commandfor</c> and <c>command="show-modal"</c> or
///     <c>command="close"</c>.
/// </summary>
[HtmlTargetElement("sa-sheet")]
public class SheetTagHelper : StellarAdminTagHelperBase
{
    private readonly IIconManager _iconManager;

    public SheetTagHelper(ICssClassMerger classMerger, IIconManager iconManager)
        : base(classMerger)
    {
        _iconManager = iconManager ?? throw new ArgumentNullException(nameof(iconManager));
    }

    /// <summary>
    ///     Whether to render the built-in close button in the corner of the sheet.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>true</c>.
    /// </remarks>
    [HtmlAttributeName("show-close-button")]
    public bool? ShowCloseButton { get; set; }

    /// <summary>
    ///     The edge of the screen the sheet slides in from.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="SheetSide.Right" />.
    /// </remarks>
    [HtmlAttributeName("side")]
    public SheetSide? Side { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveShowCloseButton = ShowCloseButton ?? true;
        var effectiveSide = Side ?? SheetSide.Right;

        output.TagName = "dialog";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sheet-content");
        output.Attributes.SetAttribute("data-side", effectiveSide.GetDataAttributeText());
        // Structural styling (including the per-side placement, keyed off data-side) lives in
        // tailwind/components.css.
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge("sa-sheet-content", output.GetUserSuppliedClass())
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
                id = $"--sa-sheet-{context.UniqueId}";
                output.Attributes.SetAttribute("id", id);
            }

            // Render the icon
            var iconOutput = new TagHelperOutput(
                "svg",
                [new TagHelperAttribute("class", "size-4")],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );
            var iconTagHelper = new IconTagHelper(ClassMerger, _iconManager) { Name = "x" };
            await iconTagHelper.ProcessAsync(context, iconOutput);

            // Render the button
            var buttonTagHelperOutput = new TagHelperOutput(
                string.Empty,
                [
                    new TagHelperAttribute("class", "sa-sheet-close"),
                    new TagHelperAttribute("commandfor", id),
                    new TagHelperAttribute("command", "close"),
                ],
                (_, _) =>
                    Task.FromResult<TagHelperContent>(
                        new DefaultTagHelperContent().AppendHtml(iconOutput)
                    )
            );
            var buttonTagHelper = new ButtonTagHelper(ClassMerger)
            {
                Size = ButtonSize.IconSmall,
                Variant = ButtonVariant.Ghost,
            };
            await buttonTagHelper.ProcessAsync(context, buttonTagHelperOutput);

            output.Content.AppendHtml(buttonTagHelperOutput);
        }

        // Add content wrapper
        var contentTagBuilder = new TagBuilder("div");
        contentTagBuilder.Attributes.Add("class", "sa-sheet-body");
        contentTagBuilder.InnerHtml.AppendHtml(await output.GetChildContentAsync());
        output.Content.AppendHtml(contentTagBuilder);
    }
}
