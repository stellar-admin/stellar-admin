using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the name of an attachment, shortened to fit.
/// </summary>
[HtmlTargetElement("sa-attachment-title")]
public class AttachmentTitleTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "attachment-title");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-attachment-title", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
