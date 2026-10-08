using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Lays out chips and the controls beside them, on the page or in an input-styled box.
/// </summary>
[HtmlTargetElement("sa-chip-group")]
public class ChipGroupTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The appearance of the group.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ChipGroupAppearance.Plain" />.
    /// </remarks>
    [HtmlAttributeName("appearance")]
    public ChipGroupAppearance? Appearance { get; set; }

    /// <summary>
    ///     Whether the chips' remove buttons are disabled.
    /// </summary>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Whether the group's value is invalid, which shows the invalid ring on an input-styled group.
    /// </summary>
    [HtmlAttributeName("invalid")]
    public bool? Invalid { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveAppearance = Appearance ?? ChipGroupAppearance.Plain;
        var effectiveDisabled = Disabled ?? false;

        SetContext(context, new ChipContext { Disabled = effectiveDisabled });

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "chip-group");
        output.Attributes.SetAttribute(
            "data-appearance",
            effectiveAppearance.GetDataAttributeText()
        );
        if (effectiveDisabled)
        {
            output.Attributes.SetAttribute("data-disabled", "true");
        }

        if (Invalid ?? false)
        {
            output.Attributes.SetAttribute("aria-invalid", "true");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-chip-group", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
