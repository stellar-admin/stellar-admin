using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The title heading of a popover.
/// </summary>
[HtmlTargetElement("sa-popover-title")]
public class PopoverTitleTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
    : StellarAdminTagHelperBase(themeManager, classMerger)
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "h2";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "popover-title");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-popover-title"),
                new ThemeToken("sa-font-heading"),
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
