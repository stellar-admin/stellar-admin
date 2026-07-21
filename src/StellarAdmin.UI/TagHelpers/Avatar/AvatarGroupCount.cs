using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A trailing element within an avatar group that displays the count of additional,
///     unshown avatars.
/// </summary>
[HtmlTargetElement("sa-avatar-group-count")]
public class AvatarGroupCount : StellarAdminTagHelperBase
{
    public AvatarGroupCount(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "avatar-group-count");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(
                new ThemeToken("sa-avatar-group-count"),
                "ring-background relative flex shrink-0 items-center justify-center ring-2"
            )
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
