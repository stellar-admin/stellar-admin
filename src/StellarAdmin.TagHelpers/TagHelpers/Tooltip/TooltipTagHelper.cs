using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

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
///     A small floating label that appears when the user hovers or focuses a trigger element,
///     rendered as a native hint popover.
/// </summary>
[HtmlTargetElement("sa-tooltip")]
public class TooltipTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The position of the tooltip relative to its anchor.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="PositionArea.Top" />.
    /// </remarks>
    [HtmlAttributeName("position")]
    public PositionArea? Position { get; set; }

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
                effectivePositionArea.GetTailwindClassName(),
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
