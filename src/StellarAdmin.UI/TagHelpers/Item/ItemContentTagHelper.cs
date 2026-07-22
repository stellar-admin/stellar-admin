using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The main content region of an item; typically wraps the title and description.
/// </summary>
[HtmlTargetElement("sa-item-content")]
public class ItemContentTagHelper : StellarAdminTagHelperBase
{
    public ItemContentTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "item-content");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge("sa-item-content", GetUserSpecifiedClass(output))
        );

        return Task.CompletedTask;
    }
}
