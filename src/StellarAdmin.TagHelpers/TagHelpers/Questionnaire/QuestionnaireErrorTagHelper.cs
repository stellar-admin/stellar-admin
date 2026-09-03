using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Displays the validation error for a questionnaire item. When bound with <c>asp-for</c>,
///     it shows the model's validation message and appears only when that answer is invalid.
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
    ///     An expression to be evaluated against the current model.
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
        var tagBuilder =
            For == null
                ? null
                : _htmlGenerator.GenerateValidationMessage(
                    ViewContext,
                    For.ModelExplorer,
                    For.Name,
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
