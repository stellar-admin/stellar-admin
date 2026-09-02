using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     An inline note in a conversation, such as a status update, a system message or a labeled divider.
/// </summary>
[HtmlTargetElement("sa-marker")]
public class MarkerTagHelper : StellarAdminTagHelperBase
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

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        MarkerRenderingHelper.Render(output, effectiveVariant);

        return Task.CompletedTask;
    }
}
