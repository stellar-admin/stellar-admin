using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A button styled to sit inside an input group, typically within an add-on.
/// </summary>
[HtmlTargetElement("sa-input-group-button")]
public class InputGroupButtonTagHelper : StellarAdminTagHelperBase
{
    private static readonly Dictionary<InputGroupButtonSize, string> SizeClasses = new Dictionary<
        InputGroupButtonSize,
        string
    >
    {
        [InputGroupButtonSize.ExtraSmall] = "sa-input-group-button-size-xs",
        [InputGroupButtonSize.Small] = "sa-input-group-button-size-sm",
        [InputGroupButtonSize.IconExtraSmall] = "sa-input-group-button-size-icon-xs",
        [InputGroupButtonSize.IconSmall] = "sa-input-group-button-size-icon-sm",
    };

    /// <summary>
    ///     The size of the button.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="InputGroupButtonSize.ExtraSmall" />.
    /// </remarks>
    [HtmlAttributeName("size")]
    public InputGroupButtonSize? Size { get; set; }

    /// <summary>
    ///     The visual style of the button.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ButtonVariant.Ghost" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public ButtonVariant? Variant { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveSize = Size ?? InputGroupButtonSize.ExtraSmall;

        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        // Input-group buttons are in-field actions (clear, copy, toggle), so they must not submit
        // the enclosing form unless the author asks for it.
        if (!output.Attributes.ContainsName("type"))
        {
            output.Attributes.SetAttribute("type", "button");
        }

        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-input-group-button",
                SizeClasses[effectiveSize],
                output.GetUserSuppliedClass()
            )
        );

        ButtonRenderingHelper.RenderAttributes(
            output,
            Variant ?? ButtonVariant.Ghost,
            ButtonSize.Default
        );

        return base.ProcessAsync(context, output);
    }
}
