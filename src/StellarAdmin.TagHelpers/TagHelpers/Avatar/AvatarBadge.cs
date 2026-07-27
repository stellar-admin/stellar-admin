using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A small badge overlaid on the corner of an avatar, such as a status indicator or icon.
/// </summary>
[HtmlTargetElement("sa-avatar-badge")]
public class AvatarBadge : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "avatar-badge");
        output.Attributes.Add(
            "class",
            JoinCssClasses("sa-avatar-badge", output.GetUserSuppliedClass())
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
