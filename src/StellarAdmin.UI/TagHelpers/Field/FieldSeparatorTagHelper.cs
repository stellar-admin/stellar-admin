using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a horizontal divider between fields.
/// </summary>
/// <remarks>
///     Content, such as "or", is shown in a gap in the middle of the line.
/// </remarks>
[HtmlTargetElement("sa-field-separator")]
public sealed class FieldSeparatorTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var childContent = await output.GetChildContentAsync();

        output.Attributes.SetAttribute("data-slot", "field-separator");
        output.Attributes.SetAttribute(
            "data-content",
            childContent.IsEmptyOrWhiteSpace ? "false" : "true"
        );
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-field-separator", output.GetUserSuppliedClass())
        );

        /* Add the actual separator */
        var separatorOutput = new TagHelperOutput(
            "",
            [new TagHelperAttribute("class", "sa-field-separator-line")],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var separatorTagHelper = new SeparatorTagHelper()
        {
            Orientation = SeparatorOrientation.Horizontal,
        };
        await separatorTagHelper.ProcessAsync(context, separatorOutput);
        output.Content.AppendHtml(separatorOutput);

        /* Add the child content, if any */
        if (!childContent.IsEmptyOrWhiteSpace)
        {
            var contentWrapperTagBuilder = new TagBuilder("span");
            contentWrapperTagBuilder.Attributes.Add("data-slot", "field-separator-content");
            contentWrapperTagBuilder.Attributes.Add(
                "class",
                JoinCssClasses("sa-field-separator-content")
            );

            contentWrapperTagBuilder.InnerHtml.AppendHtml(childContent);
            output.Content.AppendHtml(contentWrapperTagBuilder);
        }
    }
}
