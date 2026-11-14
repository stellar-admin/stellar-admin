using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an option in an <c>sa-dropdown-menu-radio-group</c>.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-radio-item")]
public sealed class DropdownMenuRadioItemTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Gets or sets a value that indicates whether clicking the item closes the menu.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if clicking the item closes the menu; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("close-on-click")]
    public bool? CloseOnClick { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the item is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the item is disabled and can't be chosen; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets the value of the item.
    /// </summary>
    /// <remarks>
    ///     The item is selected when its value matches the group's <c>value</c>.
    /// </remarks>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DropdownMenuRadioItemTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the selection mark.</param>
    public DropdownMenuRadioItemTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var group = GetContext<DropdownMenuRadioGroupContext>(context);
        var isChecked = group?.IsSelected(Value) == true;

        DropdownMenuInternals.RenderRadioItem(
            output,
            Value,
            group?.GroupName,
            isChecked,
            CloseOnClick,
            Disabled
        );

        var childContent = await output.GetChildContentAsync();
        output.Content.SetHtmlContent(
            DropdownMenuInternals.ItemIndicator(_iconOptions, "dropdown-menu-radio-item-indicator")
        );
        output.Content.AppendHtml(childContent);
    }
}
