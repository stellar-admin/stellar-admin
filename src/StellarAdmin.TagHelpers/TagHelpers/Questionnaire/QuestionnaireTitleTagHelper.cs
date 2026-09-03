using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The question a questionnaire item asks. Renders the item's <c>&lt;legend&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-questionnaire-title")]
public class QuestionnaireTitleTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "legend";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "questionnaire-title");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-questionnaire-title",
                "sa-font-heading",
                "font-heading",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
