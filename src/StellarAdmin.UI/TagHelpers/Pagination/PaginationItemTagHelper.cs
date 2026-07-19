using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A single item within the pagination list.
/// </summary>
[HtmlTargetElement("sa-pagination-item")]
public class PaginationItemTagHelper : StellarAdminTagHelperBase
{
    public PaginationItemTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "li";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "pagination-item");

        return Task.CompletedTask;
    }
}
