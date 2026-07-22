using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A styled multi-line text input that grows with its content. Supports model binding via
///     <c>asp-for</c>.
/// </summary>
[HtmlTargetElement("sa-textarea")]
public class TextareaTagHelper : FieldInputBaseTagHelper
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly ICssClassMerger _classMerger;

    public TextareaTagHelper(IHtmlGenerator htmlGenerator, ICssClassMerger classMerger)
        : base(htmlGenerator, classMerger)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _classMerger = classMerger ?? throw new ArgumentNullException(nameof(classMerger));
    }

    protected override async Task<AutoFieldConfiguration> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    )
    {
        output.TagName = "textarea";
        output.TagMode = TagMode.StartTagAndEndTag;

        var tagBuilder =
            For == null
                ? GenerateTextAreaTagBuilder(rows: 0, columns: 0, htmlAttributes: htmlAttributes)
                : _htmlGenerator.GenerateTextArea(
                    ViewContext,
                    For.ModelExplorer,
                    For.Name,
                    rows: 0,
                    columns: 0,
                    htmlAttributes: htmlAttributes
                );

        output.MergeAttributes(tagBuilder);

        if (!output.Attributes.ContainsName("data-slot"))
        {
            output.Attributes.SetAttribute("data-slot", "textarea");
        }

        output.Attributes.SetAttribute(
            "class",
            _classMerger.Merge("sa-textarea", output.GetUserSuppliedClass())
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

        return new AutoFieldConfiguration(AutoFieldLayout.Vertical);
    }

    private TagBuilder GenerateTextAreaTagBuilder(
        int rows = 0,
        int columns = 0,
        IDictionary<string, object?>? htmlAttributes = null
    )
    {
        if (rows < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows));
        }

        if (columns < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(columns));
        }

        var tagBuilder = new TagBuilder("textarea");
        if (htmlAttributes != null)
        {
            tagBuilder.MergeAttributes(htmlAttributes, true);
        }

        if (rows > 0)
        {
            tagBuilder.MergeAttribute("rows", rows.ToString(CultureInfo.InvariantCulture), true);
        }

        if (columns > 0)
        {
            tagBuilder.MergeAttribute("cols", columns.ToString(CultureInfo.InvariantCulture), true);
        }

        return tagBuilder;
    }
}
