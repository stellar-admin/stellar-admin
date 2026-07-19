using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Navigation for moving between pages of content.
/// </summary>
[HtmlTargetElement("sa-pagination")]
public class PaginationTagHelper : StellarAdminTagHelperBase
{
    public PaginationTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "nav";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "navigation");
        output.Attributes.SetAttribute("aria-label", "pagination");
        output.Attributes.SetAttribute("data-slot", "pagination");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-pagination"),
                "mx-auto flex w-full justify-center",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
