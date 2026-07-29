using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Wraps a submenu, pairing a <c>sa-dropdown-menu-sub-trigger</c> with its
///     <c>sa-dropdown-menu-sub-content</c> and generating the shared id that links them.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-sub")]
public class DropdownMenuSubTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var subId =
            output.Attributes.TryGetAttribute("id", out var idAttribute)
            && idAttribute.Value?.ToString() is { Length: > 0 } userId
                ? userId
                : $"--sa-dropdown-menu-sub-{context.UniqueId}";

        SetContext(context, new DropdownMenuContext { MenuId = subId });

        output.TagName = null;

        return Task.CompletedTask;
    }
}
