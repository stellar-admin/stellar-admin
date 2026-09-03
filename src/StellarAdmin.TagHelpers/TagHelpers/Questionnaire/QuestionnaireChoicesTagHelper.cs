using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The list of answers for a questionnaire item.
/// </summary>
[HtmlTargetElement("sa-questionnaire-choices")]
public class QuestionnaireChoicesTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Assigns a shortcut key to each choice that does not set one itself, selecting the
    ///     choice when pressed. Keys are assigned in order over the choices that can be picked,
    ///     skipping any that are disabled, and they need <c>stellar-admin.js</c>. They apply once
    ///     focus is inside the questionnaire, which clicking anywhere in the question does.
    /// </summary>
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
