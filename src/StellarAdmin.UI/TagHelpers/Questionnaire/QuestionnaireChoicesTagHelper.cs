using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the list of choices for a questionnaire item.
/// </summary>
[HtmlTargetElement("sa-questionnaire-choices")]
public class QuestionnaireChoicesTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the kind of shortcut key that each choice gets.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the kind of key, or <see langword="null" />
    ///     for no shortcut keys. The default is <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     Keys go in order to the choices that don't set their own, skipping disabled choices.
    ///     Pressing a key selects its choice once focus is inside the question, which clicking
    ///     anywhere in the question does. Requires <c>stellar-admin.js</c>.
    /// </remarks>
    [HtmlAttributeName("shortcuts")]
    public QuestionnaireShortcuts? Shortcuts { get; set; }

    /// <inheritdoc />
    protected override void OnInit(TagHelperContext context)
    {
        SetContext(context, new QuestionnaireChoicesContext { Shortcuts = Shortcuts });
    }

    /// <inheritdoc />
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
            JoinCssClasses("sa-questionnaire-choices", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
