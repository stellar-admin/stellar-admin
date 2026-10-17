using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a multi-line text input inside an input group.
/// </summary>
/// <remarks>
///     The text area blends into the group, so the group and its add-ons look like one field.
/// </remarks>
[HtmlTargetElement("sa-input-group-textarea")]
public sealed class InputGroupTextAreaTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Gets or sets the model property that the text area is bound to.
    /// </summary>
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    /// <summary>
    ///     Gets or sets the context of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InputGroupTextAreaTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders a bound text area.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public InputGroupTextAreaTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("data-slot", "input-group-control");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-input-group-textarea", output.GetUserSuppliedClass())
        );

        var textareaTagHelper = new TextareaTagHelper(_htmlGenerator)
        {
            ViewContext = ViewContext,
            For = For,
            WrapInField = false,
        };
        await textareaTagHelper.ProcessAsync(context, output);
    }
}
