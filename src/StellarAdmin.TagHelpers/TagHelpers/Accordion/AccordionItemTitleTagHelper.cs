using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The clickable header of an accordion item that toggles the item open and closed.
/// </summary>
[HtmlTargetElement("sa-accordion-item-title")]
public class AccordionItemTitleTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    public AccordionItemTitleTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "summary";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-accordion-trigger",
                "group/accordion-trigger",
                output.GetUserSuppliedClass()
            )
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
