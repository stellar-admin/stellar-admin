using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

internal static class EnvironmentIndicatorRendering
{
    public static TagBuilder CreateTab(string label, string size)
    {
        var tab = new TagBuilder("span");
        tab.AddCssClass("sa-environment-tab");
        tab.Attributes.Add("data-size", size);
        tab.InnerHtml.Append(label);

        return tab;
    }

    public static void RenderCustomColors(
        TagHelperOutput output,
        string? customColor,
        string? customDarkColor
    )
    {
        // The stylesheet uses the dark colour in dark mode and falls back to the light one
        var declarations = new List<string>();
        if (!string.IsNullOrWhiteSpace(customColor))
        {
            declarations.Add($"--_custom: {customColor}");
        }

        if (!string.IsNullOrWhiteSpace(customDarkColor))
        {
            declarations.Add($"--_custom-dark: {customDarkColor}");
        }

        if (declarations.Count > 0)
        {
            output.Attributes.SetAttribute("style", string.Join("; ", declarations));
        }
    }
}
