using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A vertically stacked set of collapsible items, each of which can be expanded to reveal its content.
/// </summary>
[HtmlTargetElement("sa-accordion")]
public class AccordionTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("data-slot", "accordion");

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-accordion", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
