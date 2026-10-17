using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a collapsible item in an accordion.
/// </summary>
/// <remarks>
///     The item renders as a native <c>&lt;details&gt;</c> element. Add the <c>open</c> attribute
///     to expand it when the page loads.
/// </remarks>
[HtmlTargetElement("sa-accordion-item")]
public sealed class AccordionItemTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "details";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "accordion-item");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-accordion-item", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
