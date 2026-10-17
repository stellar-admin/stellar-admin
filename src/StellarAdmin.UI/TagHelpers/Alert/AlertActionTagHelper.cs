using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an area in an alert for controls such as buttons or links.
/// </summary>
[HtmlTargetElement("sa-alert-action")]
public class AlertActionTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
