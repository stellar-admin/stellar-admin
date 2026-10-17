using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the button that opens and closes a dropdown menu.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-trigger")]
public class DropdownMenuTriggerTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the size of the button.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="ButtonSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public ButtonSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the button.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the visual style. The default is
    ///     <see cref="ButtonVariant.Outline" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public ButtonVariant? Variant { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("type", "button");
        output.Attributes.SetAttribute("data-slot", "dropdown-menu-trigger");
        output.Attributes.SetAttribute("aria-haspopup", "menu");

        // Native popover invoker: clicking toggles the menu and establishes the implicit
        // CSS anchor reference the content positions against.
        var menuId = GetContext<DropdownMenuContext>(context)?.MenuId;
        if (menuId != null)
        {
            output.Attributes.SetAttribute("popovertarget", menuId);
        }

        ButtonRenderingHelper.RenderAttributes(
            output,
            Variant ?? ButtonVariant.Outline,
            Size ?? ButtonSize.Default
        );

        return Task.CompletedTask;
    }
}
