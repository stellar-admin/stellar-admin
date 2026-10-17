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
public class PopoverTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the position of the popover relative to its anchor.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the position. The default is
    ///     <see cref="PositionArea.Bottom" />.
    /// </value>
    [HtmlAttributeName("position")]
    public PositionArea? Position { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var effectivePositionArea = Position ?? PositionArea.Bottom;

        if (!output.Attributes.ContainsName("popover"))
        {
            output.Attributes.SetAttribute("popover", "");
        }

        output.Attributes.SetAttribute("data-slot", "popover-content");
        output.Attributes.SetAttribute(
            "data-anchor-side",
            effectivePositionArea.GetAnchorSideDataAttributeText()
        );
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-popover-content",
                effectivePositionArea.GetClassName(),
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
