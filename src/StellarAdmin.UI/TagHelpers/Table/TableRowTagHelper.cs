using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A row within a table, rendered as a <c>&lt;tr&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-table-row")]
public class TableRowTagHelper : StellarAdminTagHelperBase
{
    public TableRowTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "tr";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-row");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(new ThemeToken("sa-table-row"), output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
