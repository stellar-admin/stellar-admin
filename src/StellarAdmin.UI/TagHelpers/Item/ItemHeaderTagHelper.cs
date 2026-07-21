using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The header region of an item, spanning its full width above the main content.
/// </summary>
[HtmlTargetElement("sa-item-header")]
public class ItemHeaderTagHelper : StellarAdminTagHelperBase
{
    public ItemHeaderTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "item-header");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-item-header"),
                "flex basis-full items-center justify-between",
                GetUserSpecifiedClass(output)
            )
        );

        return Task.CompletedTask;
    }
}
