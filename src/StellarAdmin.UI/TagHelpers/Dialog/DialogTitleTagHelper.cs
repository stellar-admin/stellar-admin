using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The title heading of a dialog.
/// </summary>
[HtmlTargetElement("sa-dialog-title")]
public class DialogTitleTagHelper : StellarAdminTagHelperBase
{
    public DialogTitleTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "h2";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "dialog-title");
        output.Attributes.Add(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-dialog-title"),
                new ThemeToken("sa-font-heading"),
                "font-heading",
                output.GetUserSuppliedClass()
            )
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
