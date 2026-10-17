using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a dropdown menu, which pairs a trigger with its content.
/// </summary>
/// <remarks>
///     Set <c>id</c> here, not on the content, to choose the menu's id. Requires
///     <c>stellar-admin.js</c>.
/// </remarks>
[HtmlTargetElement("sa-dropdown-menu")]
public class DropdownMenuTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var menuId =
            output.Attributes.TryGetAttribute("id", out var idAttribute)
            && idAttribute.Value?.ToString() is { Length: > 0 } userId
                ? userId
                : $"--sa-dropdown-menu-{GetUniqueId(context)}";

        SetContext(context, new DropdownMenuContext { MenuId = menuId });

        output.TagName = null;

        return Task.CompletedTask;
    }
}
