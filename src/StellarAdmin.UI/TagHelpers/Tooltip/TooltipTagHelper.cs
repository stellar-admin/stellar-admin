using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/*
 * Tooltip uses the interest invokers. Here are various links I found useful while developing this:
 *
 * - https://open-ui.org/components/interest-invokers.explainer/
 * - https://chrome.dev/anchor-tool/
 * - https://css-tricks.com/css-anchor-positioning-guide/
 * - https://github.com/toolwind/anchors?tab=readme-ov-file
 * - https://developer.chrome.com/blog/popover-hint
 * - https://developer.chrome.com/blog/new-in-web-ui-io-2025-recap#css_anchor_positioning
 * - https://codepen.io/una/pen/JooENdE
 * - https://github.com/mfreed7/interestfor/tree/main?tab=readme-ov-file
 */

/// <summary>
///     Renders a small label that appears when the user hovers over or focuses another element.
/// </summary>
/// <remarks>
///     The tooltip is a native popover. Give it an <c>id</c>, and set <c>interestfor</c> on the
///     element that shows it to that <c>id</c>.
/// </remarks>
[HtmlTargetElement("sa-tooltip")]
public sealed class TooltipTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets how the tooltip lines up with its trigger.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the alignment. The default is
    ///     <see cref="TooltipAlign.Center" />.
    /// </value>
    [HtmlAttributeName("align")]
    public TooltipAlign? Align { get; set; }

    /// <summary>
    ///     Gets or sets the side of its trigger that the tooltip appears on.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the side. The default is
    ///     <see cref="TooltipSide.Top" />.
    /// </value>
    [HtmlAttributeName("side")]
    public TooltipSide? Side { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var side = (Side ?? TooltipSide.Top).GetDataAttributeText();
        var align = (Align ?? TooltipAlign.Center).GetDataAttributeText();

        if (!output.Attributes.ContainsName("popover"))
        {
            output.Attributes.SetAttribute("popover", "hint");
        }

        output.Attributes.SetAttribute("data-slot", "tooltip-content");
        output.Attributes.SetAttribute("data-anchor-side", side);
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-tooltip-content",
                AnchorPlacement.GetClassName(side, align),
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
