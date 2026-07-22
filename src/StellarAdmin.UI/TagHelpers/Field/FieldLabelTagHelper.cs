using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The label for a field's control, rendered as a <c>&lt;label&gt;</c> element.
/// </summary>
[HtmlTargetElement("sa-field-label")]
public class FieldLabelTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    public FieldLabelTagHelper(IHtmlGenerator htmlGenerator, ICssClassMerger classMerger)
        : base(classMerger)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    private const string ForAttributeName = "asp-for";

    /// <summary>
    /// An expression to be evaluated against the current model.
    /// </summary>
    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression? For { get; set; }

    /// <summary>
    /// Gets the <see cref="ViewContext"/> of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("data-slot", "field-label");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-field-label"),
                "group/field-label peer/field-label",
                output.GetUserSuppliedClass()
            )
        );

        var labelTagHelper = new LabelTagHelper(_htmlGenerator, ClassMerger)
        {
            For = For,
            ViewContext = ViewContext,
        };
        await labelTagHelper.ProcessAsync(context, output);
    }
}
