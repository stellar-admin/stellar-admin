using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A horizontally scrolling row of attachments that snaps each one into view.
/// </summary>
[HtmlTargetElement("sa-attachment-group")]
public class AttachmentGroupTagHelper : StellarAdminTagHelperBase
{
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
