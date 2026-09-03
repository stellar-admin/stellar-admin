using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Supporting text explaining a single choice.
/// </summary>
[HtmlTargetElement("sa-questionnaire-choice-description")]
public class QuestionnaireChoiceDescriptionTagHelper : StellarAdminTagHelperBase
{
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
