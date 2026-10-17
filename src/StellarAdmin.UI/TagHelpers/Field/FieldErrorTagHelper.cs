using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the validation error message of a field.
/// </summary>
/// <remarks>
///     When bound with <c>asp-for</c>, it shows the model's validation message and appears only
///     when that property is invalid.
/// </remarks>
[HtmlTargetElement("sa-field-error")]
public sealed class FieldErrorTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FieldErrorTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders a bound validation message.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public FieldErrorTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    private const string ForAttributeName = "asp-for";

    /// <summary>
    ///     Gets or sets the model property whose validation message is shown.
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
        var userClass = output.GetUserSuppliedClass();

        var tagBuilder =
            For == null
                ? GenerateValidationMessageTagBuilder()
                : _htmlGenerator.GenerateValidationMessage(
                    ViewContext,
                    For.ModelExplorer,
                    For.Name,
                    null,
                    "div",
                    null
                );

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (tagBuilder != null)
        {
            output.MergeAttributes(tagBuilder);
        }

        output.Attributes.SetAttribute("role", "alert");
        output.Attributes.SetAttribute("data-slot", "field-error");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-field-error",
                tagBuilder?.Attributes.GetValueOrDefault("class"),
                userClass
            )
        );

        var childContent = await output.GetChildContentAsync();
        if (childContent.IsEmptyOrWhiteSpace)
        {
            if (tagBuilder?.HasInnerHtml == true)
            {
                output.Content.SetHtmlContent(tagBuilder.InnerHtml);
            }
        }
        else
        {
            output.Content.SetHtmlContent(childContent);
        }
    }

    private TagBuilder GenerateValidationMessageTagBuilder(
        IDictionary<string, object?>? htmlAttributes = null
    )
    {
        var tagBuilder = new TagBuilder("div");
        tagBuilder.AddCssClass(HtmlHelper.ValidationMessageCssClassName);

        if (htmlAttributes != null)
        {
            tagBuilder.MergeAttributes(htmlAttributes);
        }

        return tagBuilder;
    }
}
