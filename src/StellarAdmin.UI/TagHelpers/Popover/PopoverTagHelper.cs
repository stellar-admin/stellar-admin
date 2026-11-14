using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a floating panel of content that is anchored to the element that opens it.
/// </summary>
/// <remarks>
///     The panel is a native popover. Open it from a button whose <c>popovertarget</c> is the
///     panel's <c>id</c>.
/// </remarks>
[HtmlTargetElement("sa-popover")]
public sealed class PopoverTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets how the popover lines up with its trigger.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the alignment. The default is
    ///     <see cref="PopoverAlign.Center" />.
    /// </value>
    [HtmlAttributeName("align")]
    public PopoverAlign? Align { get; set; }

    /// <summary>
    ///     Gets or sets the side of its trigger that the popover appears on.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the side. The default is
    ///     <see cref="PopoverSide.Bottom" />.
    /// </value>
    [HtmlAttributeName("side")]
    public PopoverSide? Side { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        PopoverRenderingHelper.Render(
            output,
            Side ?? PopoverSide.Bottom,
            Align ?? PopoverAlign.Center
        );

        return Task.CompletedTask;
    }
}
