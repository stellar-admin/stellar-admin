using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

internal static class KbdRenderingHelper
{
    public static void RenderKey(TagHelperOutput output)
    {
        Render(output, "kbd", "sa-kbd");
    }

    public static void RenderGroup(TagHelperOutput output)
    {
        Render(output, "kbd-group", "sa-kbd-group");
    }

    private static void Render(TagHelperOutput output, string slot, string className)
    {
        output.TagName = "kbd";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", slot);
        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(className, output.GetUserSuppliedClass())
        );
    }
}
