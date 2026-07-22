using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The content region of an accordion item, revealed when the item is expanded.
/// </summary>
[HtmlTargetElement("sa-accordion-item-content")]
public class AccordionItemContentTagHelper : StellarAdminTagHelperBase
{
    public AccordionItemContentTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "accordion-content");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(new ThemeToken("sa-accordion-content"))
        );

        var innerTagBuilder = new TagBuilder("div");
        innerTagBuilder.Attributes.Add(
            "class",
            BuildClassString(
                new ThemeToken("sa-accordion-content-inner"),
                output.GetUserSuppliedClass()
            )
        );
        innerTagBuilder.InnerHtml.AppendHtml(await output.GetChildContentAsync());

        output.Content.AppendHtml(innerTagBuilder);
    }
}
