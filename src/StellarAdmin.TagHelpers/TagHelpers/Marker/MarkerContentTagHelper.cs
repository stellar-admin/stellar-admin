using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The text content of a marker.
/// </summary>
[HtmlTargetElement("sa-marker-content")]
public class MarkerContentTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "marker-content");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-marker-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
