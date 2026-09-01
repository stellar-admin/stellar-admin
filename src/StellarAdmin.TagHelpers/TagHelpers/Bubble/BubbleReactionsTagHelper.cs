using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A row of emoji reactions or small actions, anchored to an edge of the bubble.
/// </summary>
[HtmlTargetElement("sa-bubble-reactions")]
public class BubbleReactionsTagHelper : StellarAdminTagHelperBase
{
    private static readonly Dictionary<BubbleReactionsAlign, string> AlignClasses = new()
    {
        [BubbleReactionsAlign.Start] = "sa-bubble-reactions-align-start",
        [BubbleReactionsAlign.End] = "sa-bubble-reactions-align-end",
    };

    private static readonly Dictionary<BubbleReactionsSide, string> SideClasses = new()
    {
        [BubbleReactionsSide.Top] = "sa-bubble-reactions-side-top",
        [BubbleReactionsSide.Bottom] = "sa-bubble-reactions-side-bottom",
    };

    /// <summary>
    ///     The horizontal edge of the bubble the reactions row is anchored to.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="BubbleReactionsAlign.End" />.
    /// </remarks>
    [HtmlAttributeName("align")]
    public BubbleReactionsAlign? Align { get; set; }

    /// <summary>
    ///     The vertical edge of the bubble the reactions row is anchored to.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="BubbleReactionsSide.Bottom" />.
    /// </remarks>
    [HtmlAttributeName("side")]
    public BubbleReactionsSide? Side { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveAlign = Align ?? BubbleReactionsAlign.End;
        var effectiveSide = Side ?? BubbleReactionsSide.Bottom;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "bubble-reactions");
        output.Attributes.SetAttribute("data-align", effectiveAlign.GetDataAttributeText());
        output.Attributes.SetAttribute("data-side", effectiveSide.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-bubble-reactions",
                SideClasses[effectiveSide],
                AlignClasses[effectiveAlign],
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
