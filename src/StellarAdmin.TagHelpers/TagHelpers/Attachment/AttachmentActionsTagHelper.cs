using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The region of an attachment that holds its action controls.
/// </summary>
[HtmlTargetElement("sa-attachment-actions")]
public class AttachmentActionsTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "attachment-actions");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-attachment-actions", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
