using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The accessible title of a sheet, rendered as a heading in the sheet header.
/// </summary>
[HtmlTargetElement("sa-sheet-title")]
public class SheetTitleTagHelper(ICssClassMerger classMerger)
    : StellarAdminTagHelperBase(classMerger)
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "h2";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "sheet-title");
        output.Attributes.Add(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-sheet-title"),
                new ThemeToken("sa-font-heading"),
                "font-heading",
                output.GetUserSuppliedClass()
            )
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
