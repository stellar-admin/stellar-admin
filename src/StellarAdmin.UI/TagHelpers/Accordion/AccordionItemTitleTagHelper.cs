using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The clickable header of an accordion item that toggles the item open and closed.
/// </summary>
[HtmlTargetElement("sa-accordion-item-title")]
public class AccordionItemTitleTagHelper : StellarAdminTagHelperBase
{
    public AccordionItemTitleTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "summary";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute(
            "class",
            BuildClassString(
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
        iconTagBuilder.InnerHtml.AppendHtml(
            """
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="">
                <path d="m6 9 6 6 6-6"/>
            </svg>
            """
        );
        output.Content.AppendHtml(iconTagBuilder);
    }
}
