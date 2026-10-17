using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the caption of a field set as a <c>&lt;legend&gt;</c> element.
/// </summary>
[HtmlTargetElement("sa-field-legend")]
public class FieldLegendTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the visual style of the legend.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="FieldLegendVariant.Legend" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public FieldLegendVariant? Variant { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? FieldLegendVariant.Legend;

        output.TagName = "legend";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "field-legend");
        output.Attributes.SetAttribute("data-variant", effectiveVariant.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-field-legend", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
