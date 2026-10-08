using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A compact token for a selected or entered value, with optional media before its label and
///     an optional remove button after it.
/// </summary>
[HtmlTargetElement("sa-chip")]
public class ChipTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Whether the chip is disabled, which dims it and disables its remove button.
    /// </summary>
    /// <remarks>
    ///     Defaults to the containing <c>&lt;sa-chip-group&gt;</c>'s setting.
    /// </remarks>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

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
