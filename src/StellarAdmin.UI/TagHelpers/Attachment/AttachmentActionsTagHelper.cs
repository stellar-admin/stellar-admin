using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the area of an attachment that holds its action buttons.
/// </summary>
[HtmlTargetElement("sa-attachment-actions")]
public sealed class AttachmentActionsTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
