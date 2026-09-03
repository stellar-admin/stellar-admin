using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A single question, with its title, description, choices, and error. Renders a
///     <c>&lt;fieldset&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-questionnaire-item")]
public class QuestionnaireItemTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Whether the question accepts more than one answer. Its choices render as checkboxes
    ///     instead of radio buttons.
    /// </summary>
    [HtmlAttributeName("multiple")]
    public bool Multiple { get; set; }

    /// <summary>
    ///     Whether the question must be answered.
    /// </summary>
    [HtmlAttributeName("required")]
    public bool Required { get; set; }

    public override void Init(TagHelperContext context)
    {
        base.Init(context);

        SetContext(context, new QuestionnaireItemContext { Multiple = Multiple });
    }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "fieldset";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "questionnaire-item");

        if (Multiple)
        {
            output.Attributes.SetAttribute("data-multiple", string.Empty);
        }

        if (Required)
        {
            output.Attributes.SetAttribute("data-required", string.Empty);
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-questionnaire-item", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
