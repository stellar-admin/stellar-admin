using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The title heading of an alert dialog.
/// </summary>
[HtmlTargetElement("sa-alert-dialog-title")]
public class AlertDialogTitleTagHelper : StellarAdminTagHelperBase
{
    public AlertDialogTitleTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "h2";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "alert-dialog-title");
        output.Attributes.Add(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-alert-dialog-title"),
                new ThemeToken("sa-font-heading"),
                "font-heading",
                output.GetUserSuppliedClass()
            )
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
