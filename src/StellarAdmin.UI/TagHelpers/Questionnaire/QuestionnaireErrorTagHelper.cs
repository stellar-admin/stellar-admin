using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the validation message for a questionnaire item's answer.
/// </summary>
/// <remarks>
///     A bound item renders its message automatically. Add this element only to place the message
///     yourself or to write your own text.
/// </remarks>
[HtmlTargetElement("sa-questionnaire-error")]
public class QuestionnaireErrorTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="QuestionnaireErrorTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders the validation message of a bound item.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
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
    ///     Gets or sets the context of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <inheritdoc />
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
