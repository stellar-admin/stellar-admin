using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A vertical list container that groups related items together.
/// </summary>
[HtmlTargetElement("sa-item-group")]
public class ItemGroupTagHelper : StellarAdminTagHelperBase
{
    public ItemGroupTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "list");
        output.Attributes.SetAttribute("data-slot", "item-group");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-item-group"),
                "group/item-group",
                GetUserSpecifiedClass(output)
            )
        );

        return Task.CompletedTask;
    }
}
