using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a button inside an input group, usually in an add-on.
/// </summary>
/// <remarks>
///     The button doesn't submit its form unless it has <c>type="submit"</c>.
/// </remarks>
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
    ///     Gets or sets the size of the button.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="InputGroupButtonSize.ExtraSmall" />.
    /// </value>
    [HtmlAttributeName("size")]
    public InputGroupButtonSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the button.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="ButtonVariant.Ghost" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public ButtonVariant? Variant { get; set; }

    /// <inheritdoc />
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
