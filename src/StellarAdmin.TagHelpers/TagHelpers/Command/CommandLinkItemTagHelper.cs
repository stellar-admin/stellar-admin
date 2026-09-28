using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A command menu option rendered as a link, navigating to its target when chosen.
/// </summary>
[HtmlTargetElement("sa-command-link-item")]
public class CommandLinkItemTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Whether the item shows a check indicator, for example to mark the current page.
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

    public CommandLinkItemTagHelper(IHtmlGenerator htmlGenerator, IOptions<IconOptions> iconOptions)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconOptions = iconOptions.Value;
    }

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
