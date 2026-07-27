using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The popover panel that holds a submenu's items, positioned relative to its sub-trigger.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-sub-content")]
public class DropdownMenuSubContentTagHelper(IOptions<StellarAdminUIOptions> options)
    : StellarAdminTagHelperBase()
{
    /// <summary>
    ///     Where the submenu panel is placed relative to its sub-trigger.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="PositionArea.RightSpanBottom" />.
    /// </remarks>
    [HtmlAttributeName("position")]
    public PositionArea? Position { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var menuOptions = options.Value.Menu;
        var effectivePosition = Position ?? PositionArea.RightSpanBottom;

        output.TagName = "sel-dropdown-menu";
        output.TagMode = TagMode.StartTagAndEndTag;

        var subId = GetContext<DropdownMenuContext>(context)?.MenuId;
        if (subId != null && !output.Attributes.ContainsName("id"))
        {
            output.Attributes.SetAttribute("id", subId);
        }

        if (subId != null)
        {
            // Anchor against the sub-trigger's explicit anchor-name (the <div> sub-trigger
            // can't set up the implicit anchor a <button> popovertarget would).
            output.AppendStyle($"position-anchor: {subId}");
        }

        if (!output.Attributes.ContainsName("popover"))
        {
            output.Attributes.SetAttribute("popover", "");
        }

        output.Attributes.SetAttribute("role", "menu");
        output.Attributes.SetAttribute("data-slot", "dropdown-menu-sub-content");
        output.Attributes.SetAttribute(
            "data-anchor-side",
            effectivePosition.GetAnchorSideDataAttributeText()
        );
        output.Attributes.SetAttribute("data-sub", "");
        output.Attributes.SetAttribute(
            "data-side",
            DropdownMenuInternals.GetSideDataAttribute(effectivePosition)
        );
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-dropdown-menu-sub-content",
                "sa-dropdown-menu-subcontent",
                MenuSurfaceInternals.ColorToken(menuOptions.Color),
                MenuSurfaceInternals.AppearanceToken(menuOptions.Appearance),
                MenuSurfaceInternals.AccentToken(menuOptions.Accent),
                effectivePosition.GetTailwindClassName(),
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
