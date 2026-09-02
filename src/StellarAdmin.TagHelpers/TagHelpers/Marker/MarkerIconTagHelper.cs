using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A decorative icon alongside the marker content, hidden from assistive technology.
/// </summary>
[HtmlTargetElement("sa-marker-icon")]
public class MarkerIconTagHelper : StellarAdminTagHelperBase
{
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
