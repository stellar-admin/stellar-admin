using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A container that displays a set of avatars as an overlapping stack.
/// </summary>
[HtmlTargetElement("sa-avatar-group")]
public class AvatarGroupTagHelper : StellarAdminTagHelperBase
{
    public AvatarGroupTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "avatar-group");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString("sa-avatar-group", "group/avatar-group", output.GetUserSuppliedClass())
        );

        await base.ProcessAsync(context, output);
    }
}
