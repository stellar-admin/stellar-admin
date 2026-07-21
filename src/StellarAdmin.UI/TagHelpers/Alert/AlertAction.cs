using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A region within an alert for interactive elements such as buttons or links.
/// </summary>
[HtmlTargetElement("sa-alert-action")]
public class AlertAction : StellarAdminTagHelperBase
{
    public AlertAction(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override async void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "alert-action");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(new ThemeToken("sa-alert-action"), output.GetUserSuppliedClass())
        );

        output.Content.SetHtmlContent(await output.GetChildContentAsync());
    }
}
