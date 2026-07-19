using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The list that holds the individual pagination items.
/// </summary>
[HtmlTargetElement("sa-pagination-content")]
public class PaginationContentTagHelper : StellarAdminTagHelperBase
{
    public PaginationContentTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "ul";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "pagination-content");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-pagination-content"),
                "flex items-center",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
