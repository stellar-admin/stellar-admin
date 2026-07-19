using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The descriptive body text of an alert dialog, shown beneath the title.
/// </summary>
[HtmlTargetElement("sa-alert-dialog-description")]
public class AlertDialogDescriptionTagHelper : StellarAdminTagHelperBase
{
    public AlertDialogDescriptionTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "p";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "alert-dialog-description");
        output.Attributes.Add(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-alert-dialog-description"),
                output.GetUserSuppliedClass()
            )
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
