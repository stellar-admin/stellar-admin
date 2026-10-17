using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the description of an alert, shown below the title.
/// </summary>
[HtmlTargetElement("sa-alert-description")]
public sealed class AlertDescriptionTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
