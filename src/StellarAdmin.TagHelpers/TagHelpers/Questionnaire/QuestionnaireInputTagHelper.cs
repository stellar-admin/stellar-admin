using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using FrameworkInputTagHelper = Microsoft.AspNetCore.Mvc.TagHelpers.InputTagHelper;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A free-text answer, shown alongside an item's fixed choices. Always give it an
///     accessible name with a visible label, <c>aria-label</c>, or <c>aria-labelledby</c>.
/// </summary>
[HtmlTargetElement("sa-questionnaire-input", TagStructure = TagStructure.WithoutEndTag)]
public class QuestionnaireInputTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    public QuestionnaireInputTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <summary>
    ///     An expression to be evaluated against the current model.
    /// </summary>
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    /// <summary>
    ///     A composite format string used to format the bound model value.
    /// </summary>
    [HtmlAttributeName("asp-format")]
    public string? Format { get; set; }

    /// <summary>
    ///     The <c>type</c> of the HTML input.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>text</c>.
    /// </remarks>
    [HtmlAttributeName("type")]
    public string? InputTypeName { get; set; }

    /// <summary>
    ///     Whether to render this answer's validation message after the input. The message
    ///     reports the property this input is bound to, which is not the one the question's
    ///     choices post; set this to <c>false</c> to place the message yourself.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>true</c>.
    /// </remarks>
    [HtmlAttributeName("render-error")]
    public bool? RenderError { get; set; }

    /// <summary>
    ///     The value of the input.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <summary>
    ///     Gets the <see cref="ViewContext" /> of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var inputOutput = new TagHelperOutput(
            "input",
            new TagHelperAttributeList(output.Attributes),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        )
        {
            TagMode = TagMode.StartTagOnly,
        };

        if (For == null)
        {
            // The framework helper copies "type" straight off the host element, so it may only be
            // told about a type the author actually wrote.
            if (InputTypeName != null)
            {
                inputOutput.CopyHtmlAttribute("type", context);
            }

            if (Value != null)
            {
                inputOutput.CopyHtmlAttribute("value", context);
            }
        }
        else
        {
            var inputTagHelper = new FrameworkInputTagHelper(_htmlGenerator)
            {
                For = For,
                Format = Format,
                InputTypeName = InputTypeName,
                Value = Value,
                ViewContext = ViewContext,
            };

            inputTagHelper.Process(context, inputOutput);
        }

        if (!inputOutput.Attributes.ContainsName("type"))
        {
            inputOutput.Attributes.SetAttribute("type", "text");
        }

        inputOutput.Attributes.SetAttribute("data-slot", "questionnaire-input");
        inputOutput.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-questionnaire-input",
                inputOutput.GetUserSuppliedClass(),
                output.GetUserSuppliedClass()
            )
        );

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.Clear();
        output.Attributes.SetAttribute("data-slot", "questionnaire-input-wrapper");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-questionnaire-input-wrapper", "group/questionnaire-input")
        );
        output.Content.SetHtmlContent(inputOutput);

        // The question's own message reports the choices; this one reports the property typed
        // into here, so it sits with the input rather than at the end of the question.
        if (For != null && RenderError != false)
        {
            output.PostElement.AppendHtml(await BuildErrorAsync(context));
        }
    }

    private async Task<TagHelperOutput> BuildErrorAsync(TagHelperContext context)
    {
        var errorOutput = new TagHelperOutput(
            string.Empty,
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );

        var errorTagHelper = new QuestionnaireErrorTagHelper(_htmlGenerator)
        {
            Automatic = true,
            For = For,
            ViewContext = ViewContext,
        };

        await errorTagHelper.ProcessAsync(context, errorOutput);

        return errorOutput;
    }
}
