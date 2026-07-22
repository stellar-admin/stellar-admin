using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The popover panel that holds a dropdown menu's items, positioned relative to its trigger.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-content")]
public class DropdownMenuContentTagHelper(IOptions<StellarAdminUIOptions> options)
    : StellarAdminTagHelperBase()
{
    /// <summary>
    ///     Where the panel is placed relative to its trigger.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="PositionArea.BottomSpanRight" />.
    /// </remarks>
    [HtmlAttributeName("position")]
    public PositionArea? Position { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var menuOptions = options.Value.Menu;
        var effectivePosition = Position ?? PositionArea.BottomSpanRight;

        output.TagName = "sel-dropdown-menu";
        output.TagMode = TagMode.StartTagAndEndTag;

        var menuId = GetContext<DropdownMenuContext>(context)?.MenuId;
        if (menuId != null && !output.Attributes.ContainsName("id"))
        {
            output.Attributes.SetAttribute("id", menuId);
        }

        if (!output.Attributes.ContainsName("popover"))
        {
            output.Attributes.SetAttribute("popover", "");
        }

        output.Attributes.SetAttribute("role", "menu");
        output.Attributes.SetAttribute("data-slot", "dropdown-menu-content");
        output.Attributes.SetAttribute(
            "data-anchor-side",
            effectivePosition.GetAnchorSideDataAttributeText()
        );
        output.Attributes.SetAttribute(
            "data-side",
            DropdownMenuInternals.GetSideDataAttribute(effectivePosition)
        );
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-dropdown-menu-content",
                "sa-dropdown-menu-content-logical",
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
