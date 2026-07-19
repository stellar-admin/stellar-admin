using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The descriptive body text of an alert, shown beneath the title.
/// </summary>
[HtmlTargetElement("sa-alert-description")]
public class AlertDescriptionTagHelper : StellarAdminTagHelperBase
{
    public AlertDescriptionTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "alert-description");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(
                new ThemeToken("sa-alert-description"),
                "[&_a]:hover:text-foreground [&_a]:underline [&_a]:underline-offset-3",
                output.GetUserSuppliedClass()
            )
        );

        output.Content.SetHtmlContent(await output.GetChildContentAsync());
    }
}
