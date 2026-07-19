using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A horizontal divider used to separate items within a group.
/// </summary>
[HtmlTargetElement("sa-item-separator")]
public class ItemSeparatorTagHelper : StellarAdminTagHelperBase
{
    public ItemSeparatorTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("data-slot", "item-separator");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(new ThemeToken("sa-item-separator"), GetUserSpecifiedClass(output))
        );

        var separatorTagHelper = new SeparatorTagHelper(ThemeManager, ClassMerger)
        {
            Orientation = SeparatorOrientation.Horizontal,
        };
        await separatorTagHelper.ProcessAsync(context, output);
    }
}
