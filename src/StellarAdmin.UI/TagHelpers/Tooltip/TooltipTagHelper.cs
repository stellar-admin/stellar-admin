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
public class TooltipTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets where the tooltip appears relative to the element that shows it.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the position. The default is
    ///     <see cref="PositionArea.Top" />.
    /// </value>
    [HtmlAttributeName("position")]
    public PositionArea? Position { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var effectivePositionArea = Position ?? PositionArea.Top;

        if (!output.Attributes.ContainsName("popover"))
        {
            output.Attributes.SetAttribute("popover", "hint");
        }

        output.Attributes.SetAttribute("data-slot", "tooltip-content");
        output.Attributes.SetAttribute(
            "data-anchor-side",
            effectivePositionArea.GetAnchorSideDataAttributeText()
        );
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-tooltip-content",
                effectivePositionArea.GetClassName(),
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
