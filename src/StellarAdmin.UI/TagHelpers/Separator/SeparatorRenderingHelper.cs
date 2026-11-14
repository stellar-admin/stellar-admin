using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

internal static class SeparatorRenderingHelper
{
    public static void Render(TagHelperOutput output, SeparatorOrientation orientation)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var orientationText = orientation.GetDataAttributeText();
        output.Attributes.SetAttribute("role", "separator");
        output.Attributes.SetAttribute("aria-orientation", orientationText);
        output.Attributes.SetAttribute("data-slot", "separator");
        output.Attributes.SetAttribute("data-orientation", orientationText);
        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses("sa-separator", output.GetUserSuppliedClass())
        );
    }
}
