using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the help text of a field.
/// </summary>
/// <remarks>
///     When the element has no content and is bound with <c>asp-for</c>, it shows the description
///     from the model metadata. It renders nothing when there's no text to show.
/// </remarks>
[HtmlTargetElement("sa-field-description")]
public class FieldDescriptionTagHelper : StellarAdminTagHelperBase
{
    private const string ForAttributeName = "asp-for";

    /// <summary>
    ///     Gets or sets the model property whose description is shown.
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
        output.TagName = "p";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "field-description");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-field-description", output.GetUserSuppliedClass())
        );

        var childContent = await output.GetChildContentAsync();
        if (childContent.IsEmptyOrWhiteSpace)
        {
            if (For?.Metadata.Description is { Length: > 0 } description)
            {
                output.Content.SetContent(description);
            }
            else
            {
                // If we are trying to resolve the description via the metadata (i.e. when using asp-for)
                // but there is not description, we suppress output to prevent this control from rendering.
                // If we don't do this, it generates an empty div with a gap around it which takes up
                // unnecessary space.
                output.SuppressOutput();
            }
        }
        else
        {
            output.Content.AppendHtml(childContent);
        }
    }
}
