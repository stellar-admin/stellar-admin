using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the row that holds a questionnaire's navigation buttons.
/// </summary>
[HtmlTargetElement("sa-questionnaire-actions")]
public sealed class QuestionnaireActionsTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
