using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a row of attachments that scrolls horizontally and snaps each one into view.
/// </summary>
[HtmlTargetElement("sa-attachment-group")]
public class AttachmentGroupTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "attachment-group");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-attachment-group", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
