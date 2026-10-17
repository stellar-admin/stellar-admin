using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the panel that holds a submenu's items.
/// </summary>
/// <remarks>
///     The panel takes its id from the enclosing <c>sa-dropdown-menu-sub</c>.
/// </remarks>
[HtmlTargetElement("sa-dropdown-menu-sub-content")]
public sealed class DropdownMenuSubContentTagHelper : StellarAdminTagHelperBase
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
    ///     <see cref="DropdownMenuSide.Right" />.
    /// </value>
    [HtmlAttributeName("side")]
    public DropdownMenuSide? Side { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The panel sets an <c>id</c> that differs from the submenu's id.
    /// </exception>
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var side = (Side ?? DropdownMenuSide.Right).GetDataAttributeText();
        var align = (Align ?? DropdownMenuAlign.Start).GetDataAttributeText();

        output.TagName = "sel-dropdown-menu";
        output.TagMode = TagMode.StartTagAndEndTag;

        var subId = GetContext<DropdownMenuContext>(context)?.MenuId;
        if (subId != null)
        {
            // The trigger has already rendered against the shared id, so an id of the content's
            // own would leave the trigger pointing at nothing.
            if (
                output.Attributes.TryGetAttribute("id", out var idAttribute)
                && idAttribute.Value?.ToString() is { Length: > 0 } userId
                && userId != subId
            )
            {
                throw new InvalidOperationException(
                    $"<sa-dropdown-menu-sub-content> cannot set its own id; set id=\"{userId}\" on <sa-dropdown-menu-sub>, which shares it with the trigger."
                );
            }

            output.Attributes.SetAttribute("id", subId);
        }

        if (subId != null)
        {
            // Anchor against the sub-trigger's explicit anchor-name (the <div> sub-trigger
            // can't set up the implicit anchor a <button> popovertarget would).
            output.AppendStyle($"position-anchor: {subId}");
        }

        if (!output.Attributes.ContainsName("popover"))
        {
            output.Attributes.SetAttribute("popover", "");
        }

        output.Attributes.SetAttribute("role", "menu");
        output.Attributes.SetAttribute("data-slot", "dropdown-menu-sub-content");
        output.Attributes.SetAttribute("data-anchor-side", side);
        output.Attributes.SetAttribute("data-sub", "");
        output.Attributes.SetAttribute("data-side", side);
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-dropdown-menu-sub-content",
                "sa-dropdown-menu-subcontent",
                AnchorPlacement.GetClassName(side, align),
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
