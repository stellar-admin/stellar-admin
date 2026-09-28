using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A selectable option in a command menu. Choosing it dispatches a bubbling
///     <c>itemselect</c> event whose detail carries the item's value.
/// </summary>
[HtmlTargetElement("sa-command-item")]
public class CommandItemTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Whether the item shows a check indicator, for example to mark the current choice.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>. The indicator is hidden when the item has a shortcut.
    /// </remarks>
    [HtmlAttributeName("checked")]
    public bool? Checked { get; set; }

    /// <summary>
    ///     Whether the item is disabled. Disabled items cannot be highlighted or chosen.
    /// </summary>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Additional text that the search matches, such as synonyms.
    /// </summary>
    [HtmlAttributeName("keywords")]
    public string? Keywords { get; set; }

    /// <summary>
    ///     The value reported when the item is chosen and matched by the search.
    /// </summary>
    /// <remarks>
    ///     Defaults to the item's text.
    /// </remarks>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    public CommandItemTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

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
