using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

internal static class BubbleContentRenderingHelper
{
    public static void Render(TagHelperOutput output)
    {
        output.Attributes.SetAttribute("data-slot", "bubble-content");
        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(
                "sa-bubble-content",
                output.GetUserSuppliedClass()
            )
        );
    }
}
