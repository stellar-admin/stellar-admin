using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

internal static class MarkerRenderingHelper
{
    public static void Render(TagHelperOutput output, MarkerVariant variant)
    {
        output.Attributes.SetAttribute("data-slot", "marker");
        output.Attributes.SetAttribute("data-variant", variant.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses("sa-marker", output.GetUserSuppliedClass())
        );
    }
}
