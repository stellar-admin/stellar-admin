using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a command menu item as a link.
/// </summary>
/// <remarks>
///     Choosing the item follows the link.
/// </remarks>
[HtmlTargetElement("sa-command-item-link")]
public class CommandItemLinkTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Gets or sets a value that indicates whether the item shows a check mark.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the item shows a check mark, for example to mark the
    ///     current page; otherwise, <see langword="false" />. The default is
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
    ///     Initializes a new instance of the <see cref="CommandItemLinkTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that builds the link's URL.</param>
    /// <param name="iconOptions">The icon options that supply the check mark.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public CommandItemLinkTagHelper(IHtmlGenerator htmlGenerator, IOptions<IconOptions> iconOptions)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconOptions = iconOptions.Value;
    }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";

        await ApplyRouteAttributesAsync(_htmlGenerator, context, output);

        CommandRenderingHelper.RenderItem(
            context,
            output,
            _iconOptions,
            Value,
            Keywords,
            Disabled,
            Checked
        );
    }
}
