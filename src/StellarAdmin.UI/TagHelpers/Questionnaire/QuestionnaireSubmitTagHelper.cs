using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a button that completes the questionnaire.
/// </summary>
/// <remarks>
///     The button submits the form, so place it inside your own <c>&lt;form&gt;</c>. With no
///     content, it shows "Submit".
/// </remarks>
[HtmlTargetElement("sa-questionnaire-submit")]
public sealed class QuestionnaireSubmitTagHelper : StellarAdminTagHelperBase
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
    ///     One of the enumeration values that specifies the visual style. The default is
    ///     <see cref="ButtonVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public ButtonVariant? Variant { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        return QuestionnaireActionRenderingHelper.RenderAsync(
            output,
            "questionnaire-submit",
            "sa-questionnaire-submit",
            Variant ?? ButtonVariant.Default,
            Size ?? ButtonSize.Default,
            "Submit"
        );
    }
}
