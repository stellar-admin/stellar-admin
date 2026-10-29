using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a vertical stack of items that each expand to show their content.
/// </summary>
[HtmlTargetElement("sa-accordion")]
public sealed class AccordionTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the visual style of the accordion.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="AccordionVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public AccordionVariant? Variant { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? AccordionVariant.Default;

        output.TagName = "div";
        output.Attributes.SetAttribute("data-slot", "accordion");
        output.Attributes.SetAttribute("data-variant", effectiveVariant.GetDataAttributeText());

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-accordion",
                effectiveVariant == AccordionVariant.Contained
                    ? "sa-accordion-variant-contained"
                    : null,
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
