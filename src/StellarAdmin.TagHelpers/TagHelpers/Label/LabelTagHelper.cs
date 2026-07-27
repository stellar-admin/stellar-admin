using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A caption for a form control, optionally bound to a model expression via <c>asp-for</c>.
/// </summary>
[HtmlTargetElement("sa-label")]
public class LabelTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    public LabelTagHelper(IHtmlGenerator htmlGenerator)
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
        output.TagName = "label";
        output.TagMode = TagMode.StartTagAndEndTag;

        var tagBuilder =
            For == null
                ? new TagBuilder("label")
                : _htmlGenerator.GenerateLabel(
                    ViewContext,
                    For.ModelExplorer,
                    For.Name,
                    labelText: null,
                    htmlAttributes: null
                );

        output.MergeAttributes(tagBuilder);

        if (!output.Attributes.ContainsName("data-slot"))
        {
            output.Attributes.SetAttribute("data-slot", "label");
        }
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-label", output.GetUserSuppliedClass())
        );

        var childContent = await output.GetChildContentAsync();
        if (childContent.IsEmptyOrWhiteSpace)
        {
            if (tagBuilder.HasInnerHtml)
            {
                output.Content.SetHtmlContent(tagBuilder.InnerHtml);
            }
        }
        else
        {
            output.Content.SetHtmlContent(childContent);
        }
    }
}
