using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the panel that holds a dropdown menu's items.
/// </summary>
/// <remarks>
///     The panel takes its id from the enclosing <c>sa-dropdown-menu</c>.
/// </remarks>
/// <param name="options">The UI options that supply the menu's color, appearance and accent.</param>
[HtmlTargetElement("sa-dropdown-menu-content")]
public sealed class DropdownMenuContentTagHelper(IOptions<StellarAdminUIOptions> options)
    : StellarAdminTagHelperBase()
{
    /// <summary>
    ///     Gets or sets how the panel lines up with its trigger.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the alignment. The default is
    ///     <see cref="DropdownMenuAlign.Start" />.
    /// </value>
    [HtmlAttributeName("align")]
    public DropdownMenuAlign? Align { get; set; }

    /// <summary>
    ///     Gets or sets the side of its trigger that the panel appears on.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the side. The default is
    ///     <see cref="DropdownMenuSide.Bottom" />.
    /// </value>
    [HtmlAttributeName("side")]
    public DropdownMenuSide? Side { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The panel sets an <c>id</c> that differs from the menu's id.
    /// </exception>
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var menuOptions = options.Value.Menu;
        var side = (Side ?? DropdownMenuSide.Bottom).GetDataAttributeText();
        var align = (Align ?? DropdownMenuAlign.Start).GetDataAttributeText();

        output.TagName = "sel-dropdown-menu";
        output.TagMode = TagMode.StartTagAndEndTag;

        var menuId = GetContext<DropdownMenuContext>(context)?.MenuId;
        if (menuId != null)
        {
            // The trigger has already rendered against the shared id, so an id of the content's
            // own would leave the trigger pointing at nothing.
            if (
                output.Attributes.TryGetAttribute("id", out var idAttribute)
                && idAttribute.Value?.ToString() is { Length: > 0 } userId
                && userId != menuId
            )
            {
                throw new InvalidOperationException(
                    $"<sa-dropdown-menu-content> cannot set its own id; set id=\"{userId}\" on <sa-dropdown-menu>, which shares it with the trigger."
                );
            }

            output.Attributes.SetAttribute("id", menuId);
        }

        if (!output.Attributes.ContainsName("popover"))
        {
            output.Attributes.SetAttribute("popover", "");
        }

        output.Attributes.SetAttribute("role", "menu");
        output.Attributes.SetAttribute("data-slot", "dropdown-menu-content");
        output.Attributes.SetAttribute("data-anchor-side", side);
        output.Attributes.SetAttribute("data-side", side);
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-dropdown-menu-content",
                "sa-dropdown-menu-content-logical",
                MenuSurfaceInternals.ColorToken(menuOptions.Color),
                MenuSurfaceInternals.AppearanceToken(menuOptions.Appearance),
                MenuSurfaceInternals.AccentToken(menuOptions.Accent),
                AnchorPlacement.GetClassName(side, align),
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
