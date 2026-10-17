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
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var side = (Side ?? PopoverSide.Bottom).GetDataAttributeText();
        var align = (Align ?? PopoverAlign.Center).GetDataAttributeText();

        if (!output.Attributes.ContainsName("popover"))
        {
            output.Attributes.SetAttribute("popover", "");
        }

        output.Attributes.SetAttribute("data-slot", "popover-content");
        output.Attributes.SetAttribute("data-anchor-side", side);
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-popover-content",
                AnchorPlacement.GetClassName(side, align),
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
