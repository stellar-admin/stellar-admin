using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
    ///     An expression to be evaluated against the current model, naming the answer to this
    ///     question. Its choices take their name and selected state from it, and its error shows
    ///     that answer's validation message.
    /// </summary>
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    /// <summary>
    ///     Whether the question accepts more than one answer. Its choices render as checkboxes
    ///     instead of radio buttons.
    /// </summary>
    [HtmlAttributeName("multiple")]
    public bool Multiple { get; set; }

    /// <summary>
    ///     The name the answer posts under. Set automatically when bound with <c>asp-for</c>.
    /// </summary>
    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    /// <summary>
    ///     Whether the question must be answered.
    /// </summary>
    [HtmlAttributeName("required")]
    public bool Required { get; set; }

    public override void Init(TagHelperContext context)
    {
        base.Init(context);

        // The question owns the answer, so the choices and the error read the expression from
        // here rather than each repeating it. This has to happen in Init: the children run
        // before the item's own ProcessAsync does.
        SetContext(
            context,
            new QuestionnaireItemContext
            {
                For = For,
                Multiple = Multiple,
                Name = Name,
            }
        );
    }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "fieldset";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "questionnaire-item");

        // Focusable by click but not by tab: clicking anywhere in the question - its title, its
        // description, the space around them - focuses it, which is what puts its shortcut keys
        // in reach without adding a tab stop of its own.
        if (!output.Attributes.ContainsName("tabindex"))
        {
            output.Attributes.SetAttribute("tabindex", "-1");
        }

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
