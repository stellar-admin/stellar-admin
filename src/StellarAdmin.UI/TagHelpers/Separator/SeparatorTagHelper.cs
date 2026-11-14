using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a thin line between sections of content.
/// </summary>
[HtmlTargetElement("sa-separator")]
public sealed class SeparatorTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the orientation of the separator.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the orientation. The default is
    ///     <see cref="SeparatorOrientation.Horizontal" />.
    /// </value>
    [HtmlAttributeName("orientation")]
    public SeparatorOrientation? Orientation { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        SeparatorRenderingHelper.Render(output, Orientation ?? SeparatorOrientation.Horizontal);

        return Task.CompletedTask;
    }
}
