using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a question, with its title, description, choices and validation message.
/// </summary>
[HtmlTargetElement("sa-questionnaire-item")]
public sealed class QuestionnaireItemTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="QuestionnaireItemTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders the validation message of a bound question.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public QuestionnaireItemTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <summary>
    ///     Gets or sets the model property that holds the answer.
    /// </summary>
    /// <remarks>
    ///     The choices take their name and selected state from the property. The item shows the
    ///     property's validation message.
    /// </remarks>
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the question accepts more than one answer.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the question accepts more than one answer; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     The choices of a multiple-answer question are checkboxes instead of radio buttons.
    /// </remarks>
    [HtmlAttributeName("multiple")]
    public bool? Multiple { get; set; }

    /// <summary>
    ///     Gets or sets the name that the answer posts under.
    /// </summary>
    /// <value>
    ///     A form field name. The default is the name of the bound property.
    /// </value>
    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the answer's validation message shows at
    ///     the end of the question.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the message shows; otherwise, <see langword="false" />.
    ///     The default is <see langword="true" />.
    /// </value>
    /// <remarks>
    ///     Applies only when bound with <c>asp-for</c>. The message is left out when the item holds
    ///     its own <c>&lt;sa-questionnaire-error&gt;</c>.
    /// </remarks>
    [HtmlAttributeName("show-error")]
    public bool? ShowError { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the question must be answered.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the question must be answered; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     This only marks the question. Validate the answer on the model.
    /// </remarks>
    [HtmlAttributeName("required")]
    public bool? Required { get; set; }

    /// <summary>
    ///     Gets or sets the context of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <inheritdoc />
    protected override void OnInit(TagHelperContext context)
    {
        // The question owns the answer, so the choices and the error read the expression from
        // here rather than each repeating it. This has to happen in Init: the children run
        // before the item's own ProcessAsync does.
        SetContext(
            context,
            new QuestionnaireItemContext
            {
                For = For,
                Multiple = Multiple ?? false,
                Name = Name,
            }
        );
    }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
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

        if (Multiple ?? false)
        {
            output.Attributes.SetAttribute("data-multiple", string.Empty);
        }

        if (Required ?? false)
        {
            output.Attributes.SetAttribute("data-required", string.Empty);
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-questionnaire-item", output.GetUserSuppliedClass())
        );

        if (For == null || ShowError == false)
        {
            return;
        }

        // A bound question carries its own message, so the author has nothing to remember. The
        // children have not run yet - child content is lazy, and a question that only decorates
        // never asks for it - so run them here: an error the author placed records itself on the
        // way past and this stands down rather than rendering a second one. Asking for the
        // content means writing it back, since it no longer renders on its own.
        output.Content.SetHtmlContent(await output.GetChildContentAsync());

        if (GetContext<QuestionnaireItemContext>(context)?.ErrorRendered != true)
        {
            output.PostContent.AppendHtml(await BuildErrorAsync(context));
        }
    }

    private async Task<TagHelperOutput> BuildErrorAsync(TagHelperContext context)
    {
        var errorOutput = new TagHelperOutput(
            string.Empty,
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );

        // Rendered through the tag helper so the automatic message and a hand-placed one cannot
        // drift apart. It reads the answer off the same item context the children did.
        var errorTagHelper = new QuestionnaireErrorTagHelper(_htmlGenerator)
        {
            Automatic = true,
            ViewContext = ViewContext,
        };

        await errorTagHelper.ProcessAsync(context, errorOutput);

        return errorOutput;
    }
}
