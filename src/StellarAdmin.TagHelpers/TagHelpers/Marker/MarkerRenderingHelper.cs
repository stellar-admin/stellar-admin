using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

internal static class MarkerRenderingHelper
{
    private static readonly Dictionary<MarkerVariant, string> MarkerVariantClasses = new()
    {
        [MarkerVariant.Default] = "sa-marker-variant-default",
        [MarkerVariant.Border] = "sa-marker-variant-border",
        [MarkerVariant.Separator] = "sa-marker-variant-separator",
    };

    public static void Render(TagHelperOutput output, MarkerVariant variant)
    {
        output.Attributes.SetAttribute("data-slot", "marker");
        output.Attributes.SetAttribute("data-variant", variant.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(
                "sa-marker",
                "group/marker",
                MarkerVariantClasses[variant],
                output.GetUserSuppliedClass()
            )
        );
    }
}
