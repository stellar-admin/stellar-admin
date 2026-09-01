using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A framed message in a conversation, holding its content and any reactions.
/// </summary>
[HtmlTargetElement("sa-bubble")]
public class BubbleTagHelper : StellarAdminTagHelperBase
{
    private static readonly Dictionary<BubbleVariant, string> BubbleVariantClasses = new()
    {
        [BubbleVariant.Default] = "sa-bubble-variant-default",
        [BubbleVariant.Secondary] = "sa-bubble-variant-secondary",
        [BubbleVariant.Muted] = "sa-bubble-variant-muted",
        [BubbleVariant.Tinted] = "sa-bubble-variant-tinted",
        [BubbleVariant.Outline] = "sa-bubble-variant-outline",
        [BubbleVariant.Ghost] = "sa-bubble-variant-ghost",
        [BubbleVariant.Destructive] = "sa-bubble-variant-destructive",
    };

    /// <summary>
    ///     The side of the conversation the bubble sits on.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="BubbleAlign.Start" />.
    /// </remarks>
    [HtmlAttributeName("align")]
    public BubbleAlign? Align { get; set; }

    /// <summary>
    ///     The visual style of the bubble.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="BubbleVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public BubbleVariant? Variant { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveAlign = Align ?? BubbleAlign.Start;
        var effectiveVariant = Variant ?? BubbleVariant.Default;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "bubble");
        output.Attributes.SetAttribute("data-variant", effectiveVariant.GetDataAttributeText());
        output.Attributes.SetAttribute("data-align", effectiveAlign.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-bubble",
                "group/bubble",
                BubbleVariantClasses[effectiveVariant],
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
