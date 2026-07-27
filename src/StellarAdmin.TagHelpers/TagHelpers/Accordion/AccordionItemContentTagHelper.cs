using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The content region of an accordion item, revealed when the item is expanded.
/// </summary>
[HtmlTargetElement("sa-accordion-item-content")]
public class AccordionItemContentTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "accordion-content");
        output.Attributes.SetAttribute("class", JoinCssClasses("sa-accordion-content"));

        var innerTagBuilder = new TagBuilder("div");
        innerTagBuilder.Attributes.Add(
            "class",
            JoinCssClasses("sa-accordion-content-inner", output.GetUserSuppliedClass())
        );
        innerTagBuilder.InnerHtml.AppendHtml(await output.GetChildContentAsync());

        output.Content.AppendHtml(innerTagBuilder);
    }
}
