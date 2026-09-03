using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Displays the validation message for a questionnaire item's answer. A bound item renders
///     one automatically, so place this only to position the message yourself or to write your
///     own text.
/// </summary>
[HtmlTargetElement("sa-questionnaire-error")]
public class QuestionnaireErrorTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    public QuestionnaireErrorTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// Set on the messages the item and the input render for themselves, so that they are not
    /// mistaken for one the author placed.
    internal bool Automatic { get; set; }

    /// The answer to report, for the input rendering its own message. Otherwise the answer the
    /// item is bound to.
    internal ModelExpression? For { get; set; }

    /// <summary>
    ///     Gets the <see cref="ViewContext" /> of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var itemContext = GetContext<QuestionnaireItemContext>(context);
        var expression = For ?? itemContext?.For;

        // Placing one by hand is how the author takes the message over, so record it before the
        // item runs and renders a second.
        if (!Automatic && itemContext != null)
        {
            itemContext.ErrorRendered = true;
        }

        var tagBuilder =
            expression == null
                ? null
                : _htmlGenerator.GenerateValidationMessage(
                    ViewContext,
                    expression.ModelExplorer,
                    expression.Name,
                    null,
                    "div",
                    null
                );

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (tagBuilder != null)
        {
            output.MergeAttributes(tagBuilder);
        }

        output.Attributes.SetAttribute("role", "alert");
        output.Attributes.SetAttribute("data-slot", "questionnaire-error");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-questionnaire-error", output.GetUserSuppliedClass())
        );

        var childContent = await output.GetChildContentAsync();

        if (!childContent.IsEmptyOrWhiteSpace)
        {
            output.Content.SetHtmlContent(childContent);
        }
        else if (tagBuilder is { HasInnerHtml: true })
        {
            output.Content.SetHtmlContent(tagBuilder.InnerHtml);
        }
    }
}
