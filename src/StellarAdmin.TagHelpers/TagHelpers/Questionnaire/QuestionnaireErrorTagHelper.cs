using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Displays the validation error for a questionnaire item. When the item is bound with
///     <c>asp-for</c>, it shows that answer's validation message and appears only when the
///     answer is invalid.
/// </summary>
[HtmlTargetElement("sa-questionnaire-error")]
public class QuestionnaireErrorTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    public QuestionnaireErrorTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <summary>
    ///     An expression to be evaluated against the current model. Defaults to the expression
    ///     the item is bound to; set it to report a different answer, such as the one a
    ///     <c>sa-questionnaire-input</c> posts.
    /// </summary>
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    /// <summary>
    ///     Gets the <see cref="ViewContext" /> of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var expression = For ?? GetContext<QuestionnaireItemContext>(context)?.For;

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
