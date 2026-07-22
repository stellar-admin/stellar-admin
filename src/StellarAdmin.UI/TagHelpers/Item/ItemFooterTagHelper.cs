using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The footer region of an item, spanning its full width beneath the main content.
/// </summary>
[HtmlTargetElement("sa-item-footer")]
public class ItemFooterTagHelper : StellarAdminTagHelperBase
{
    public ItemFooterTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "item-footer");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(new ThemeToken("sa-item-footer"), GetUserSpecifiedClass(output))
        );

        return Task.CompletedTask;
    }
}
