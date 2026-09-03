using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The row holding a questionnaire's navigation buttons.
/// </summary>
[HtmlTargetElement("sa-questionnaire-actions")]
public class QuestionnaireActionsTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "questionnaire-actions");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-questionnaire-actions", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
