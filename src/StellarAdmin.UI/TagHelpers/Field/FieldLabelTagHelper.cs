using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the label of a field's control as a <c>&lt;label&gt;</c> element.
/// </summary>
/// <remarks>
///     When bound with <c>asp-for</c>, the label targets the bound control and shows the
///     property's display name if it has no content.
/// </remarks>
[HtmlTargetElement("sa-field-label")]
public sealed class FieldLabelTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FieldLabelTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders the label of a bound property.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public FieldLabelTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    private const string ForAttributeName = "asp-for";

    /// <summary>
    ///     Gets or sets the model property that the label is for.
    /// </summary>
    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression? For { get; set; }

    /// <summary>
    ///     Gets or sets the context of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("data-slot", "field-label");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-field-label", output.GetUserSuppliedClass())
        );

        var labelTagHelper = new LabelTagHelper(_htmlGenerator)
        {
            For = For,
            ViewContext = ViewContext,
        };
        await labelTagHelper.ProcessAsync(context, output);
    }
}
