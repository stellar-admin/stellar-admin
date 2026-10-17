using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a marker as a button, so the whole marker can be clicked.
/// </summary>
/// <remarks>
///     The button has <c>type="button"</c> unless the element sets its own <c>type</c>.
/// </remarks>
[HtmlTargetElement("sa-marker-button")]
public sealed class MarkerButtonTagHelper : StellarAdminTagHelperBase
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
