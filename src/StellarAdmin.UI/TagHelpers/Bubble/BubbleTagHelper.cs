using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a message in a conversation, with its content and any reactions.
/// </summary>
[HtmlTargetElement("sa-bubble")]
public sealed class BubbleTagHelper : StellarAdminTagHelperBase
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
    ///     Gets or sets the side of the conversation that the bubble sits on.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the side. The default is
    ///     <see cref="BubbleAlign.Start" />.
    /// </value>
    [HtmlAttributeName("align")]
    public BubbleAlign? Align { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the bubble.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="BubbleVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public BubbleVariant? Variant { get; set; }

    /// <inheritdoc />
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
                BubbleVariantClasses[effectiveVariant],
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
