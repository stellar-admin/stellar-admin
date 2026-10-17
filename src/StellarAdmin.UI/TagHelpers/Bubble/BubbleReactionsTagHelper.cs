using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a row of emoji reactions or small actions on an edge of a bubble.
/// </summary>
[HtmlTargetElement("sa-bubble-reactions")]
public sealed class BubbleReactionsTagHelper : StellarAdminTagHelperBase
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
    ///     Gets or sets the horizontal edge of the bubble that the reactions sit at.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the edge. The default is
    ///     <see cref="BubbleReactionsAlign.End" />.
    /// </value>
    [HtmlAttributeName("align")]
    public BubbleReactionsAlign? Align { get; set; }

    /// <summary>
    ///     Gets or sets the vertical edge of the bubble that the reactions sit on.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the edge. The default is
    ///     <see cref="BubbleReactionsSide.Bottom" />.
    /// </value>
    [HtmlAttributeName("side")]
    public BubbleReactionsSide? Side { get; set; }

    /// <inheritdoc />
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
