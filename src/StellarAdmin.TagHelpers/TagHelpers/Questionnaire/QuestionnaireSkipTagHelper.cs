using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Leaves the current question unanswered and moves on. Renders a submit button, so place it inside your own <c>&lt;form&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-questionnaire-skip")]
public class QuestionnaireSkipTagHelper : StellarAdminTagHelperBase
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
    ///     Defaults to <see cref="ButtonVariant.Outline" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public ButtonVariant? Variant { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        return QuestionnaireActionRenderingHelper.RenderAsync(
            output,
            "questionnaire-skip",
            "sa-questionnaire-skip",
            Variant ?? ButtonVariant.Outline,
            Size ?? ButtonSize.Default,
            "Skip"
        );
    }
}
