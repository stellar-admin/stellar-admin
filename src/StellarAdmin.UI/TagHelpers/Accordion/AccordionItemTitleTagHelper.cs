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

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "summary";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-accordion-trigger", output.GetUserSuppliedClass())
        );

        // Render the content
        output.Content.AppendHtml(await output.GetChildContentAsync());

        // Render the icon
        var iconTagBuilder = new TagBuilder("div");
        iconTagBuilder.AddCssClass("sa-accordion-trigger-icon");
        var iconOutput = new TagHelperOutput(
            "svg",
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var iconTagHelper = new IconTagHelper(_iconOptions)
        {
            Name = _iconOptions.GetSemanticIconName(SemanticIconRole.AccordionIndicator),
        };
        await iconTagHelper.ProcessAsync(context, iconOutput);
        iconTagBuilder.InnerHtml.AppendHtml(iconOutput);
        output.Content.AppendHtml(iconTagBuilder);
    }
}
