using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a group of chips and the controls beside them.
/// </summary>
[HtmlTargetElement("sa-chip-group")]
public class ChipGroupTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets how the group looks.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies how the group looks. The default is
    ///     <see cref="ChipGroupAppearance.Plain" />.
    /// </value>
    [HtmlAttributeName("appearance")]
    public ChipGroupAppearance? Appearance { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the chips in the group are disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the chips are dimmed and their remove buttons are disabled;
    ///     otherwise, <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     A chip can override this with its own <c>disabled</c> attribute.
    /// </remarks>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the group's value is invalid.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the value is invalid; otherwise, <see langword="false" />. The
    ///     default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     An input-styled group shows an invalid ring.
    /// </remarks>
    [HtmlAttributeName("aria-invalid")]
    public bool? AriaInvalid { get; set; }

    /// <inheritdoc />
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

        if (AriaInvalid ?? false)
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
