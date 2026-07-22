using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A floating panel of rich content anchored to a trigger element, rendered as a native
///     popover.
/// </summary>
[HtmlTargetElement("sa-popover")]
public class PopoverTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The position of the popover relative to its anchor.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="PositionArea.Bottom" />.
    /// </remarks>
    [HtmlAttributeName("position")]
    public PositionArea? Position { get; set; }

    public PopoverTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

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
            ClassMerger.Merge(
                new ThemeToken("sa-popover-content"),
                effectivePositionArea.GetTailwindClassName(),
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
