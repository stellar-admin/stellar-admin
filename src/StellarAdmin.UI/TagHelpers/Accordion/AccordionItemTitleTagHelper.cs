using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the title of an accordion item, which expands and collapses the item when clicked.
/// </summary>
[HtmlTargetElement("sa-accordion-item-title")]
public sealed class AccordionItemTitleTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AccordionItemTitleTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the expand indicator.</param>
    public AccordionItemTitleTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Gets or sets the name of an icon shown in a tile before the title.
    /// </summary>
    /// <value>
    ///     The name of the icon, or <see langword="null" /> to show no icon. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     The item's content is indented to line up with the title.
    /// </remarks>
    [HtmlAttributeName("icon")]
    public string? Icon { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "summary";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-accordion-trigger", output.GetUserSuppliedClass())
        );

        // Render the content, after the icon's tile when there is one
        var content = await output.GetChildContentAsync();
        if (string.IsNullOrWhiteSpace(Icon))
        {
            output.Content.AppendHtml(content);
        }
        else
        {
            var mediaTagBuilder = new TagBuilder("span");
            mediaTagBuilder.AddCssClass("sa-accordion-trigger-media");
            mediaTagBuilder.InnerHtml.AppendHtml(await RenderIconAsync(context, Icon));

            var labelTagBuilder = new TagBuilder("span");
            labelTagBuilder.AddCssClass("sa-accordion-trigger-label");
            labelTagBuilder.InnerHtml.AppendHtml(mediaTagBuilder);
            labelTagBuilder.InnerHtml.AppendHtml(content);
            output.Content.AppendHtml(labelTagBuilder);
        }

        // Render the indicator
        var iconTagBuilder = new TagBuilder("div");
        iconTagBuilder.AddCssClass("sa-accordion-trigger-icon");
        iconTagBuilder.InnerHtml.AppendHtml(
            await RenderIconAsync(
                context,
                _iconOptions.GetSemanticIconName(SemanticIconRole.AccordionIndicator)
            )
        );
        output.Content.AppendHtml(iconTagBuilder);
    }

    private async Task<TagHelperOutput> RenderIconAsync(TagHelperContext context, string? name)
    {
        var iconOutput = new TagHelperOutput(
            "svg",
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var iconTagHelper = new IconTagHelper(_iconOptions) { Name = name };
        await iconTagHelper.ProcessAsync(context, iconOutput);

        return iconOutput;
    }
}
