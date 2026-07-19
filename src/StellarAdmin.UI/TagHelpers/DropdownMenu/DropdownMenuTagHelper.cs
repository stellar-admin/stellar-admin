using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The root of a dropdown menu, pairing a trigger with its content and generating the shared id
///     that links them.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu")]
public class DropdownMenuTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
    : StellarAdminTagHelperBase(themeManager, classMerger)
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var menuId =
            output.Attributes.TryGetAttribute("id", out var idAttribute)
            && idAttribute.Value?.ToString() is { Length: > 0 } userId
                ? userId
                : $"--sa-dropdown-menu-{context.UniqueId}";

        SetContext(context, new DropdownMenuContext { MenuId = menuId });

        output.TagName = null;
        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
