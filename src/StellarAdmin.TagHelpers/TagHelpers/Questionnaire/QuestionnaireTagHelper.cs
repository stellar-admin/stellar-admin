using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A container for one or more questionnaire items. Place it inside your own
///     <c>&lt;form&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-questionnaire")]
public class QuestionnaireTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "questionnaire");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-questionnaire", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
