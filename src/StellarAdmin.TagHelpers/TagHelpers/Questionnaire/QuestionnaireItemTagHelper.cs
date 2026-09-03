using Microsoft.AspNetCore.Mvc.Rendering;
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
    private readonly IHtmlGenerator _htmlGenerator;

    public QuestionnaireItemTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

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
    ///     Whether to render the answer's validation message at the end of the question. A bound
    ///     question renders one unless a <c>sa-questionnaire-error</c> inside it already does;
    ///     set this to <c>false</c> to leave the message out.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>true</c>.
    /// </remarks>
    [HtmlAttributeName("render-error")]
    public bool? RenderError { get; set; }

    /// <summary>
    ///     Whether the question must be answered.
    /// </summary>
    [HtmlAttributeName("required")]
    public bool Required { get; set; }

    /// <summary>
    ///     Gets the <see cref="ViewContext" /> of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

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

        if (For == null || RenderError == false)
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
