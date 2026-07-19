using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A data cell within a table row, rendered as a <c>&lt;td&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-table-cell")]
public class TableCellTagHelper : StellarAdminTagHelperBase
{
    public TableCellTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "td";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-cell");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(new ThemeToken("sa-table-cell"), output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
