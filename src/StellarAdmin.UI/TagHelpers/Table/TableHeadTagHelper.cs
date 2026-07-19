using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A header cell within a table header row, rendered as a <c>&lt;th&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-table-head")]
public class TableHeadTagHelper : StellarAdminTagHelperBase
{
    public TableHeadTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "th";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-head");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(new ThemeToken("sa-table-head"), output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
