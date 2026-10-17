using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a button that moves back to the previous question.
/// </summary>
/// <remarks>
///     The button submits the form, so place it inside your own <c>&lt;form&gt;</c>. With no
///     content, it shows "Previous".
/// </remarks>
[HtmlTargetElement("sa-questionnaire-previous")]
public sealed class QuestionnairePreviousTagHelper : StellarAdminTagHelperBase
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
    ///     <see cref="ButtonVariant.Outline" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public ButtonVariant? Variant { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        return QuestionnaireActionRenderingHelper.RenderAsync(
            output,
            "questionnaire-previous",
            "sa-questionnaire-previous",
            Variant ?? ButtonVariant.Outline,
            Size ?? ButtonSize.Default,
            "Previous"
        );
    }
}
