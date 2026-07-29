using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A region within an alert for interactive elements such as buttons or links.
/// </summary>
[HtmlTargetElement("sa-alert-action")]
public class AlertAction : StellarAdminTagHelperBase
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "alert-action");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-alert-action", output.GetUserSuppliedClass())
        );
    }
}
