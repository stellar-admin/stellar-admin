using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a multi-line text input that grows with its content.
/// </summary>
/// <remarks>
///     Bind it to a model property with <c>asp-for</c>. Content inside the element replaces the
///     bound value.
/// </remarks>
[HtmlTargetElement("sa-textarea")]
public sealed class TextareaTagHelper : FieldInputBaseTagHelper<TextareaClassNames>
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TextareaTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">
    ///     The generator that renders the bound textarea and the field's label and messages.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public TextareaTagHelper(IHtmlGenerator htmlGenerator)
        : base(htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <inheritdoc />
    protected override async Task<FieldLayout> RenderInput(
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
        ApplyFieldAttributes(context, output.Attributes, FieldLayout.Stacked);

        if (!output.Attributes.ContainsName("data-slot"))
        {
            output.Attributes.SetAttribute("data-slot", "textarea");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-textarea", ClassNames?.Control, output.GetUserSuppliedClass())
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

        return FieldLayout.Stacked;
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
