using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The region of an item that holds action controls such as buttons, aligned to its trailing edge.
/// </summary>
[HtmlTargetElement("sa-item-actions")]
public class ItemActionsTagHelper : StellarAdminTagHelperBase
{
    public ItemActionsTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "item-actions");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-item-actions"),
                "flex items-center",
                GetUserSpecifiedClass(output)
            )
        );

        return Task.CompletedTask;
    }
}
