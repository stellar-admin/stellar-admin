using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

internal static class ItemRenderingHelper
{
    public static void Render(TagHelperOutput output, ItemSize? size, ItemVariant? variant)
    {
        var effectiveSize = size ?? ItemSize.Default;
        var effectiveVariant = variant ?? ItemVariant.Default;

        output.Attributes.SetAttribute("data-slot", "item");
        output.Attributes.SetAttribute("data-variant", effectiveVariant.GetDataAttributeText());
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());

        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses("sa-item", output.GetUserSuppliedClass())
        );
    }
}
