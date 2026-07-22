using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The descriptive body text of an alert, shown beneath the title.
/// </summary>
[HtmlTargetElement("sa-alert-description")]
public class AlertDescriptionTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "alert-description");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-alert-description", output.GetUserSuppliedClass())
        );

        output.Content.SetHtmlContent(await output.GetChildContentAsync());
    }
}
