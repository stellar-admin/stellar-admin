using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A single collapsible item within an accordion, rendered as a native
///     <c>&lt;details&gt;</c> element with a title and content region.
/// </summary>
[HtmlTargetElement("sa-accordion-item")]
public class AccordionItemTagHelper : StellarAdminTagHelperBase
{
    public AccordionItemTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "details";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "accordion-item");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(
                new ThemeToken("sa-accordion-item"),
                "group",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
