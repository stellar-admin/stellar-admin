using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The name of an attachment, truncated to fit and shimmering while it uploads or processes.
/// </summary>
[HtmlTargetElement("sa-attachment-title")]
public class AttachmentTitleTagHelper : StellarAdminTagHelperBase
{
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
