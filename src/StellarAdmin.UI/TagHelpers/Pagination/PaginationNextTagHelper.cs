using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a pagination link to the next page.
/// </summary>
/// <remarks>
///     With no content, the link shows an arrow icon and the label "Next".
/// </remarks>
[HtmlTargetElement("sa-pagination-next")]
public sealed class PaginationNextTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Gets or sets the size of the link.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="ButtonSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public ButtonSize? Size { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PaginationNextTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that builds the link URL from the routing attributes.</param>
    /// <param name="iconOptions">The icon options that supply the default arrow icon.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public PaginationNextTagHelper(IHtmlGenerator htmlGenerator, IOptions<IconOptions> iconOptions)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconOptions = iconOptions.Value;
    }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        return PaginationNavRenderingHelper.RenderAsync(
            context,
            output,
            PaginationNavRenderingHelper.CreateLink(_htmlGenerator, this, Size),
            PaginationNavDirection.Next,
            _iconOptions
        );
    }
}
