using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the number of avatars that an avatar group doesn't show, at the end of the group.
/// </summary>
[HtmlTargetElement("sa-avatar-group-count")]
public class AvatarGroupCountTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
