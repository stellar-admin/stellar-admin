using Microsoft.AspNetCore.Razor.TagHelpers;

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
        output.Attributes.SetAttribute("class", BuildClassString("sa-avatar-group-count"));

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
