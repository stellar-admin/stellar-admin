using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

internal static class InputRenderingHelper
{
    // Styles an <input> by its type: a checkbox, a radio button or a text-like input
    public static void RenderAttributes(
        TagHelperOutput output,
        string? type,
        params string?[] additionalClasses
    )
    {
        if (!output.Attributes.ContainsName("data-slot"))
        {
            output.Attributes.SetAttribute("data-slot", "input");
        }

        var typeClass = type switch
        {
            "checkbox" => "sa-checkbox",
            "radio" => "sa-radiobutton",
            _ => "sa-input",
        };
        string?[] classes = [typeClass, output.GetUserSuppliedClass(), .. additionalClasses];
        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(classes.Distinct().ToArray())
        );
    }
}
