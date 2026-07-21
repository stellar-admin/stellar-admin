using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The title heading of an alert.
/// </summary>
[HtmlTargetElement("sa-alert-title")]
public class AlertTitleTagHelper : StellarAdminTagHelperBase
{
    public AlertTitleTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "alert-title");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(
                new ThemeToken("sa-alert-title"),
                "[&_a]:hover:text-foreground [&_a]:underline [&_a]:underline-offset-3",
                output.GetUserSuppliedClass()
            )
        );

        output.Content.SetHtmlContent(await output.GetChildContentAsync());
    }
}
