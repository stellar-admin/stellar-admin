using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Completes the questionnaire. Renders a submit button, so place it inside your own <c>&lt;form&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-questionnaire-submit")]
public class QuestionnaireSubmitTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The size of the button.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ButtonSize.Default" />.
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
