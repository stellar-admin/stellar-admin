using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a keyboard shortcut hint at the end of a dropdown menu item.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-shortcut")]
public sealed class DropdownMenuShortcutTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        DropdownMenuInternals.RenderShortcut(output);

        return Task.CompletedTask;
    }
}
