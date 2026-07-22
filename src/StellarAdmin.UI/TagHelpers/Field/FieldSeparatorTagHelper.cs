using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A horizontal divider between fields, optionally with content (such as a label) centered on the line.
/// </summary>
[HtmlTargetElement("sa-field-separator")]
public class FieldSeparatorTagHelper : StellarAdminTagHelperBase
{
    public FieldSeparatorTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

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
            ClassMerger.Merge("sa-field-separator", output.GetUserSuppliedClass())
        );

        /* Add the actual separator */
        var separatorOutput = new TagHelperOutput(
            "",
            [new TagHelperAttribute("class", "sa-field-separator-line")],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var separatorTagHelper = new SeparatorTagHelper(ClassMerger)
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
                ClassMerger.Merge("sa-field-separator-content")
            );

            contentWrapperTagBuilder.InnerHtml.AppendHtml(childContent);
            output.Content.AppendHtml(contentWrapperTagBuilder);
        }
    }
}
