using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The footer region of a dialog; typically contains action buttons.
/// </summary>
[HtmlTargetElement("sa-dialog-footer")]
public class DialogFooterTagHelper : StellarAdminTagHelperBase
{
    public DialogFooterTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "dialog-footer");
        output.Attributes.Add(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-dialog-footer"),
                "flex flex-col-reverse gap-2 sm:flex-row sm:justify-end",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
