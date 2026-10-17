using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a container for one or more questionnaire items.
/// </summary>
/// <remarks>
///     Place it inside your own <c>&lt;form&gt;</c>. The container handles the shortcut keys, the
///     arrow keys that move between a question's answers, and a free-text answer that replaces
///     the choices. Requires <c>stellar-admin.js</c>.
/// </remarks>
[HtmlTargetElement("sa-questionnaire")]
public class QuestionnaireTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "sel-questionnaire";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "questionnaire");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-questionnaire", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
