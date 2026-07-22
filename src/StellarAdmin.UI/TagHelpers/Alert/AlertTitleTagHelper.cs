using Microsoft.AspNetCore.Razor.TagHelpers;

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
            BuildClassString("sa-alert-title", output.GetUserSuppliedClass())
        );

        output.Content.SetHtmlContent(await output.GetChildContentAsync());
    }
}
