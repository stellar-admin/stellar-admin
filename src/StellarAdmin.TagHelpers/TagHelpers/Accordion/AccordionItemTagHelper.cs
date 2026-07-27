using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A single collapsible item within an accordion, rendered as a native
///     <c>&lt;details&gt;</c> element with a title and content region.
/// </summary>
[HtmlTargetElement("sa-accordion-item")]
public class AccordionItemTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "details";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "accordion-item");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-accordion-item", "group", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
