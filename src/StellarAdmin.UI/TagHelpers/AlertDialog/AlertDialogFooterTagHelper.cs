using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The footer region of an alert dialog; typically contains the cancel and action buttons.
/// </summary>
[HtmlTargetElement("sa-alert-dialog-footer")]
public class AlertDialogFooterTagHelper : StellarAdminTagHelperBase
{
    public AlertDialogFooterTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "alert-dialog-footer");
        output.Attributes.Add(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-alert-dialog-footer"),
                "flex flex-col-reverse gap-2 group-data-[size=sm]/alert-dialog-content:grid group-data-[size=sm]/alert-dialog-content:grid-cols-2 sm:flex-row sm:justify-end",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
