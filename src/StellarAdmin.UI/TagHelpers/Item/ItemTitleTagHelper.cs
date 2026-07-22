using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The primary title text of an item.
/// </summary>
[HtmlTargetElement("sa-item-title")]
public class ItemTitleTagHelper : StellarAdminTagHelperBase
{
    public ItemTitleTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "item-title");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(new ThemeToken("sa-item-title"), GetUserSpecifiedClass(output))
        );

        return Task.CompletedTask;
    }
}
