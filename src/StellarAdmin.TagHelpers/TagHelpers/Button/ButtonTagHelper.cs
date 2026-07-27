using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Renders a button element for triggering actions.
/// </summary>
[HtmlTargetElement("sa-button")]
public class ButtonTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The size of the button.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ButtonSize.Default" />
    /// </remarks>
    [HtmlAttributeName("size")]
    public ButtonSize? Size { get; set; }

    /// <summary>
    ///     The button variant.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ButtonVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public ButtonVariant? Variant { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveSize = Size ?? ButtonSize.Default;
        var effectiveVariant = Variant ?? ButtonVariant.Default;

        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        ButtonRenderingHelper.RenderAttributes(output, effectiveVariant, effectiveSize);

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
