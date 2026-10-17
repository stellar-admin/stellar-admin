using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a set of avatars as an overlapping stack.
/// </summary>
[HtmlTargetElement("sa-avatar-group")]
public sealed class AvatarGroupTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "avatar-group");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-avatar-group", output.GetUserSuppliedClass())
        );

        await base.ProcessAsync(context, output);
    }
}
