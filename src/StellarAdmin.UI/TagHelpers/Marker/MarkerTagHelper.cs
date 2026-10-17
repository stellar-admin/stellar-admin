using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a note in a conversation, such as a status update, a system message or a labeled
///     divider.
/// </summary>
[HtmlTargetElement("sa-marker")]
public class MarkerTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the layout of the marker.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the layout. The default is
    ///     <see cref="MarkerVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public MarkerVariant? Variant { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? MarkerVariant.Default;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        MarkerRenderingHelper.Render(output, effectiveVariant);

        return Task.CompletedTask;
    }
}
