using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The region of an attachment that holds its title and description.
/// </summary>
[HtmlTargetElement("sa-attachment-content")]
public class AttachmentContentTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "attachment-content");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-attachment-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
