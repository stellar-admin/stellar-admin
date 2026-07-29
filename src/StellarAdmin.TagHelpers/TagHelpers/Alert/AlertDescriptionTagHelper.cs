using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The descriptive body text of an alert, shown beneath the title.
/// </summary>
[HtmlTargetElement("sa-alert-description")]
public class AlertDescriptionTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "alert-description");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-alert-description", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
