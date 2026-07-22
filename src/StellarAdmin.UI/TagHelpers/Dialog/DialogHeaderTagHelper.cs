using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The header region of a dialog; typically contains the title and description.
/// </summary>
[HtmlTargetElement("sa-dialog-header")]
public class DialogHeaderTagHelper : StellarAdminTagHelperBase
{
    public DialogHeaderTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "dialog-header");
        output.Attributes.Add(
            "class",
            ClassMerger.Merge(new ThemeToken("sa-dialog-header"), output.GetUserSuppliedClass())
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
