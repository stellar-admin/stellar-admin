using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders text that explains a choice.
/// </summary>
[HtmlTargetElement("sa-questionnaire-choice-description")]
public sealed class QuestionnaireChoiceDescriptionTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "questionnaire-choice-description");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-questionnaire-choice-description", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
