using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a small badge on the corner of an avatar, such as a status indicator or icon.
/// </summary>
[HtmlTargetElement("sa-avatar-badge")]
public class AvatarBadgeTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "avatar-badge");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-avatar-badge", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
