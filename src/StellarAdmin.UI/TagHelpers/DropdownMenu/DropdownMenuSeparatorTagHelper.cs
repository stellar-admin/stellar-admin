using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a line between groups of dropdown menu items.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-separator")]
public sealed class DropdownMenuSeparatorTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        DropdownMenuInternals.RenderSeparator(output);

        return Task.CompletedTask;
    }
}
