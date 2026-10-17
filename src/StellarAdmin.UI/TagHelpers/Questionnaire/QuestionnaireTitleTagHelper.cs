using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the question that a questionnaire item asks, as the item's <c>&lt;legend&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-questionnaire-title")]
public sealed class QuestionnaireTitleTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "legend";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "questionnaire-title");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-questionnaire-title", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
