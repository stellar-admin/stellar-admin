using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A horizontal divider used to separate items within a group.
/// </summary>
[HtmlTargetElement("sa-item-separator")]
public class ItemSeparatorTagHelper : StellarAdminTagHelperBase
{
    public ItemSeparatorTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("data-slot", "item-separator");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge("sa-item-separator", GetUserSpecifiedClass(output))
        );

        var separatorTagHelper = new SeparatorTagHelper(ClassMerger)
        {
            Orientation = SeparatorOrientation.Horizontal,
        };
        await separatorTagHelper.ProcessAsync(context, output);
    }
}
