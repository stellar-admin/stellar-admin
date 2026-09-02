using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A marker rendered as a button, making the whole marker activatable.
/// </summary>
[HtmlTargetElement("sa-marker-button")]
public class MarkerButtonTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The layout of the marker.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="MarkerVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public MarkerVariant? Variant { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? MarkerVariant.Default;

        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (!output.Attributes.ContainsName("type"))
        {
            output.Attributes.SetAttribute("type", "button");
        }

        MarkerRenderingHelper.Render(output, effectiveVariant);

        return Task.CompletedTask;
    }
}
