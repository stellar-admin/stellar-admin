using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

internal static class PopoverRenderingHelper
{
    public static void Render(TagHelperOutput output, PopoverSide side, PopoverAlign align)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var sideText = side.GetDataAttributeText();
        if (!output.Attributes.ContainsName("popover"))
        {
            output.Attributes.SetAttribute("popover", "");
        }

        output.Attributes.SetAttribute("data-slot", "popover-content");
        output.Attributes.SetAttribute("data-anchor-side", sideText);
        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(
                "sa-popover-content",
                AnchorPlacement.GetClassName(sideText, align.GetDataAttributeText()),
                output.GetUserSuppliedClass()
            )
        );
    }
}
