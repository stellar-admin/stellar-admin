using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a compact token for a selected or entered value.
/// </summary>
/// <remarks>
///     Media can go before the chip's label, and an <c>sa-chip-remove</c> button after it.
/// </remarks>
[HtmlTargetElement("sa-chip")]
public class ChipTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets a value that indicates whether the chip is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the chip is dimmed and its remove button is disabled; otherwise,
    ///     <see langword="false" />. The default is the setting of the chip group that contains it.
    /// </value>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveDisabled = Disabled ?? GetContext<ChipContext>(context)?.Disabled ?? false;

        SetContext(context, new ChipContext { Disabled = effectiveDisabled });

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "chip");
        if (effectiveDisabled)
        {
            output.Attributes.SetAttribute("data-disabled", "true");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-chip", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
