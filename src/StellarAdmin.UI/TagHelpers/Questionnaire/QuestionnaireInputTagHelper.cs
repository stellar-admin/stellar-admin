using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using FrameworkInputTagHelper = Microsoft.AspNetCore.Mvc.TagHelpers.InputTagHelper;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a free-text answer beside a questionnaire item's choices.
/// </summary>
/// <remarks>
///     Give the input an accessible name with a visible label, <c>aria-label</c> or
///     <c>aria-labelledby</c>.
/// </remarks>
[HtmlTargetElement("sa-questionnaire-input", TagStructure = TagStructure.WithoutEndTag)]
public class QuestionnaireInputTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="QuestionnaireInputTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders the input of a bound model property.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public QuestionnaireInputTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <summary>
    ///     Gets or sets the model property that the input is bound to.
    /// </summary>
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    /// <summary>
    ///     Gets or sets the format string for the bound property's value.
    /// </summary>
    /// <remarks>
    ///     Applies only when bound with <c>asp-for</c>.
    /// </remarks>
    [HtmlAttributeName("asp-format")]
    public string? Format { get; set; }

    /// <summary>
    ///     Gets or sets the <c>type</c> of the input.
    /// </summary>
    /// <value>
    ///     An HTML input type. The default is <c>text</c>, or the type that suits the bound
    ///     property.
    /// </value>
    [HtmlAttributeName("type")]
    public string? InputTypeName { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the validation message for the input shows
    ///     after it.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the message shows after the input; otherwise,
    ///     <see langword="false" />. The default is <see langword="true" />.
    /// </value>
    /// <remarks>
    ///     Applies only when bound with <c>asp-for</c>. The message is for the input's own property,
    ///     not the one the choices post. Set this to <see langword="false" /> to place the message
    ///     yourself.
    /// </remarks>
    [HtmlAttributeName("show-error")]
    public bool? ShowError { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the input and the item's choices replace
    ///     each other.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if only one of the input and the choices posts; otherwise,
    ///     <see langword="false" /> to post both. The default is <see langword="true" />, or
    ///     <see langword="false" /> when the item accepts multiple answers.
    /// </value>
    /// <remarks>
    ///     Typing in the input clears the selected choice, and selecting a choice stops the input
    ///     from posting. Requires <c>stellar-admin.js</c>.
    /// </remarks>
    [HtmlAttributeName("replaces-choices")]
    public bool? ReplacesChoices { get; set; }

    /// <summary>
    ///     Gets or sets the value of the input.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <summary>
    ///     Gets or sets the context of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <inheritdoc />
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

        // A question that takes one answer takes it from one place, so the free text and the
        // choices stand in for each other. A question that takes several is already collecting
        // more than one answer, and the free text adds to them.
        if (ReplacesChoices ?? !(GetContext<QuestionnaireItemContext>(context)?.Multiple ?? false))
        {
            inputOutput.Attributes.SetAttribute("data-replaces-choices", "");
        }

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
        output.Attributes.SetAttribute("class", JoinCssClasses("sa-questionnaire-input-wrapper"));
        output.Content.SetHtmlContent(inputOutput);

        // The question's own message reports the choices; this one reports the property typed
        // into here, so it sits with the input rather than at the end of the question.
        if (For != null && ShowError != false)
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
