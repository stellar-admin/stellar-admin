using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a submenu, which pairs an <c>sa-dropdown-menu-sub-trigger</c> with its
///     <c>sa-dropdown-menu-sub-content</c>.
/// </summary>
/// <remarks>
///     Set <c>id</c> here, not on the content, to choose the submenu's id.
/// </remarks>
[HtmlTargetElement("sa-dropdown-menu-sub")]
public class DropdownMenuSubTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var subId =
            output.Attributes.TryGetAttribute("id", out var idAttribute)
            && idAttribute.Value?.ToString() is { Length: > 0 } userId
                ? userId
                : $"--sa-dropdown-menu-sub-{GetUniqueId(context)}";

        SetContext(context, new DropdownMenuContext { MenuId = subId });

        output.TagName = null;

        return Task.CompletedTask;
    }
}
