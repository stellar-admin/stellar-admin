using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     The supporting text of an attachment, such as its file type, size, or upload status.
/// </summary>
[HtmlTargetElement("sa-attachment-description")]
public class AttachmentDescriptionTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "attachment-description");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-attachment-description", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
