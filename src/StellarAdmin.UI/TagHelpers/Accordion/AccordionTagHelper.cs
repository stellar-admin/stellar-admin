using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a vertical stack of items that each expand to show their content.
/// </summary>
[HtmlTargetElement("sa-accordion")]
public sealed class AccordionTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
