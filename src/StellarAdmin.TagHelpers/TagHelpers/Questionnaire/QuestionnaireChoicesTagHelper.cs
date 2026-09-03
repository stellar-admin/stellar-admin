using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The list of answers for a questionnaire item.
/// </summary>
[HtmlTargetElement("sa-questionnaire-choices")]
public class QuestionnaireChoicesTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Assigns a shortcut badge to each choice that does not set one itself.
    /// </summary>
    /// <remarks>
    ///     The badge is presentational: bind the keys yourself if you want them to select a choice.
    /// </remarks>
    [HtmlAttributeName("shortcuts")]
    public QuestionnaireShortcuts? Shortcuts { get; set; }

    public override void Init(TagHelperContext context)
    {
        base.Init(context);

        SetContext(context, new QuestionnaireChoicesContext { Shortcuts = Shortcuts });
    }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "questionnaire-choices");

        if (Shortcuts is { } shortcuts)
        {
            output.Attributes.SetAttribute("data-shortcuts", shortcuts.GetDataAttributeText());
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-questionnaire-choices",
                "group/questionnaire-choices",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
