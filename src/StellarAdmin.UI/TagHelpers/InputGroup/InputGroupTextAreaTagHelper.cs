using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A multi-line text input rendered inside an input group, styled to blend into the group.
/// </summary>
[HtmlTargetElement("sa-input-group-textarea")]
public class InputGroupTextAreaTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

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

    public InputGroupTextAreaTagHelper(IHtmlGenerator htmlGenerator, ICssClassMerger classMerger)
        : base(classMerger)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("data-slot", "input-group-control");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-input-group-textarea"),
                "flex-1 resize-none",
                output.GetUserSuppliedClass()
            )
        );

        var textareaTagHelper = new TextareaTagHelper(_htmlGenerator, ClassMerger)
        {
            ViewContext = ViewContext,
            For = For,
            ShouldRenderField = false,
        };
        await textareaTagHelper.ProcessAsync(context, output);
    }
}
