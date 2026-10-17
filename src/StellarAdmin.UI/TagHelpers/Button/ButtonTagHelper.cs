using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a button that triggers an action.
/// </summary>
/// <remarks>
///     Like a native button, it submits its form unless <c>type</c> is set.
/// </remarks>
[HtmlTargetElement("sa-button")]
public sealed class ButtonTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the size of the button.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="ButtonSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public ButtonSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the button.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="ButtonVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public ButtonVariant? Variant { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveSize = Size ?? ButtonSize.Default;
        var effectiveVariant = Variant ?? ButtonVariant.Default;

        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        ButtonRenderingHelper.RenderAttributes(output, effectiveVariant, effectiveSize);

        return Task.CompletedTask;
    }
}
