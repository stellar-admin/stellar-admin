using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the title of an alert.
/// </summary>
[HtmlTargetElement("sa-alert-title")]
public class AlertTitleTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "alert-title");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-alert-title", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
