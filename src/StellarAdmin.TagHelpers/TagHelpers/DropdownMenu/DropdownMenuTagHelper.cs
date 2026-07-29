using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The root of a dropdown menu, pairing a trigger with its content and generating the shared id
///     that links them.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu")]
public class DropdownMenuTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var menuId =
            output.Attributes.TryGetAttribute("id", out var idAttribute)
            && idAttribute.Value?.ToString() is { Length: > 0 } userId
                ? userId
                : $"--sa-dropdown-menu-{context.UniqueId}";

        SetContext(context, new DropdownMenuContext { MenuId = menuId });

        output.TagName = null;

        return Task.CompletedTask;
    }
}
