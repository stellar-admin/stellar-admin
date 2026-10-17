using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the description of an attachment, such as its file type, size or upload status.
/// </summary>
[HtmlTargetElement("sa-attachment-description")]
public sealed class AttachmentDescriptionTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
