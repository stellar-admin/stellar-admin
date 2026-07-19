using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The header section of a table, rendered as a <c>&lt;thead&gt;</c>; contains the header row.
/// </summary>
[HtmlTargetElement("sa-table-header")]
public class TableHeaderTagHelper : StellarAdminTagHelperBase
{
    public TableHeaderTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "thead";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-header");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(new ThemeToken("sa-table-header"), output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
