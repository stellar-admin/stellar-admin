using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A horizontal divider used to separate items within a group.
/// </summary>
[HtmlTargetElement("sa-item-separator")]
public class ItemSeparatorTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("data-slot", "item-separator");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-item-separator", GetUserSpecifiedClass(output))
        );

        var separatorTagHelper = new SeparatorTagHelper()
        {
            Orientation = SeparatorOrientation.Horizontal,
        };
        await separatorTagHelper.ProcessAsync(context, output);
    }
}
