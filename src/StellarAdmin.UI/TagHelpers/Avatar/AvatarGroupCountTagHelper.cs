using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     A trailing element within an avatar group that displays the count of additional,
///     unshown avatars.
/// </summary>
[HtmlTargetElement("sa-avatar-group-count")]
public class AvatarGroupCountTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "avatar-group-count");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-avatar-group-count", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
