using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

internal static class TooltipRenderingHelper
{
    public static void Render(TagHelperOutput output, TooltipSide side, TooltipAlign align)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var sideText = side.GetDataAttributeText();
        if (!output.Attributes.ContainsName("popover"))
        {
            output.Attributes.SetAttribute("popover", "hint");
        }

        output.Attributes.SetAttribute("data-slot", "tooltip-content");
        output.Attributes.SetAttribute("data-anchor-side", sideText);
        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(
                "sa-tooltip-content",
                AnchorPlacement.GetClassName(sideText, align.GetDataAttributeText()),
                output.GetUserSuppliedClass()
            )
        );
    }
}
