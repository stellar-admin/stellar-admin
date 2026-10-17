using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a decorative icon beside the text of a marker.
/// </summary>
/// <remarks>
///     The icon is hidden from assistive technology.
/// </remarks>
[HtmlTargetElement("sa-marker-icon")]
public class MarkerIconTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "marker-icon");
        output.Attributes.SetAttribute("aria-hidden", "true");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-marker-icon", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
