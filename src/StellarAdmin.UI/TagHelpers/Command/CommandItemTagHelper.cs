using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an option in a command menu.
/// </summary>
/// <remarks>
///     Choosing the item raises a bubbling <c>itemselect</c> event with the item's value in
///     <c>detail.value</c>.
/// </remarks>
[HtmlTargetElement("sa-command-item")]
public sealed class CommandItemTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Gets or sets a value that indicates whether the item shows a check mark.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the item shows a check mark, for example to mark the
    ///     current choice; otherwise, <see langword="false" />. The default is
    ///     <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     The check mark is hidden when the item has a shortcut.
    /// </remarks>
    [HtmlAttributeName("checked")]
    public bool? Checked { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the item is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the item can't be highlighted or chosen; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets additional text that the search matches, such as synonyms.
    /// </summary>
    [HtmlAttributeName("keywords")]
    public string? Keywords { get; set; }

    /// <summary>
    ///     Gets or sets the value that the search matches and that the item reports when chosen.
    /// </summary>
    /// <value>
    ///     The default is the item's text without its shortcut.
    /// </value>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandItemTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the check mark.</param>
    public CommandItemTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";

        CommandRenderingHelper.RenderItem(
            context,
            output,
            _iconOptions,
            Value,
            Keywords,
            Disabled,
            Checked
        );

        return Task.CompletedTask;
    }
}
