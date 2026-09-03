using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Moves on to the next question. Renders a submit button, so place it inside your own <c>&lt;form&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-questionnaire-next")]
public class QuestionnaireNextTagHelper : StellarAdminTagHelperBase
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
            "questionnaire-next",
            "sa-questionnaire-next",
            Variant ?? ButtonVariant.Default,
            Size ?? ButtonSize.Default,
            "Next"
        );
    }
}
