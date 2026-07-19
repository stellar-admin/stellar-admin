using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A caption for a table, rendered as a <c>&lt;caption&gt;</c>; describes the table's
///     contents.
/// </summary>
[HtmlTargetElement("sa-table-caption")]
public class TableCaptionTagHelper : StellarAdminTagHelperBase
{
    public TableCaptionTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "caption";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-caption");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(new ThemeToken("sa-table-caption"), output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
