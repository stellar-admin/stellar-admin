using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Shared helpers for the Dropdown Menu family: the popover-content positioning
///     classes (mirroring <see cref="PopoverTagHelper" />) and indicator/chevron icon
///     rendering.
/// </summary>
internal static class DropdownMenuInternals
{
    // The shared popover-surface styling (positioning, overflow, open/close transition —
    // including the justified divergences from shadcn documented there) lives in
    // tailwind/components.css under .sa-dropdown-menu-content / .sa-dropdown-menu-sub-content.

    /// <summary>
    ///     Best-effort <c>data-side</c> from the requested placement. On viewport-collision
    ///     flip (<c>try-flip-all</c>) the rendered side can differ — acceptable, since the
    ///     directional slide animation is not used (see the surface rules in tailwind/components.css).
    /// </summary>
    public static string GetSideDataAttribute(PositionArea area) =>
        area switch
        {
            PositionArea.TopCenter
            or PositionArea.TopSpanLeft
            or PositionArea.TopSpanRight
            or PositionArea.TopLeft
            or PositionArea.TopRight
            or PositionArea.Top => "top",
            PositionArea.LeftCenter
            or PositionArea.LeftSpanTop
            or PositionArea.LeftSpanBottom
            or PositionArea.Left => "left",
            PositionArea.RightCenter
            or PositionArea.RightSpanTop
            or PositionArea.RightSpanBottom
            or PositionArea.Right => "right",
            _ => "bottom",
        };

    /// <summary>Renders a Lucide icon (via <see cref="IconTagHelper" />) as inline content.</summary>
    public static IHtmlContent RenderIcon(
        TagHelperContext context,
        IIconManager iconManager,
        string name,
        string cssClass
    )
    {
        var iconOutput = new TagHelperOutput(
            string.Empty,
            [new TagHelperAttribute("class", cssClass)],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var iconTagHelper = new IconTagHelper(iconManager) { Name = name };
        iconTagHelper.Process(context, iconOutput);

        return iconOutput;
    }
}
