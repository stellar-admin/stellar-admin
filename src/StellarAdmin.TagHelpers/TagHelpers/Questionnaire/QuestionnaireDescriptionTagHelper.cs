using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Supporting text explaining a questionnaire item.
/// </summary>
[HtmlTargetElement("sa-questionnaire-description")]
public class QuestionnaireDescriptionTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "questionnaire-description");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-questionnaire-description", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
