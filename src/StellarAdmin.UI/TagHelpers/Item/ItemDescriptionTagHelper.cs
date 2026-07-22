using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The secondary descriptive text of an item, rendered beneath its title.
/// </summary>
[HtmlTargetElement("sa-item-description")]
public class ItemDescriptionTagHelper : StellarAdminTagHelperBase
{
    public ItemDescriptionTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "p";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "item-description");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(new ThemeToken("sa-item-description"), GetUserSpecifiedClass(output))
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
