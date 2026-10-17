using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a pagination link to a page.
/// </summary>
[HtmlTargetElement("sa-pagination-link")]
public class PaginationLinkTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PaginationLinkTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that builds the link URL from the routing attributes.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public PaginationLinkTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <summary>
    ///     Gets or sets a value that indicates whether the link is for the current page.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the link is for the current page; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     The current page's link is highlighted and announced to screen readers.
    /// </remarks>
    [HtmlAttributeName("active")]
    public bool? Active { get; set; }

    /// <summary>
    ///     Gets or sets the size of the link.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="ButtonSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public ButtonSize? Size { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveActive = Active ?? false;
        var effectiveSize = Size ?? ButtonSize.Default;

        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        await ApplyRouteAttributesAsync(_htmlGenerator, context, output);

        if (effectiveActive)
        {
            output.Attributes.SetAttribute("aria-current", "page");
        }
        output.Attributes.SetAttribute("data-slot", "pagination-link");
        output.Attributes.SetAttribute("data-active", effectiveActive ? "true" : "false");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-pagination-link", output.GetUserSuppliedClass())
        );

        ButtonRenderingHelper.RenderAttributes(
            output,
            effectiveActive ? ButtonVariant.Outline : ButtonVariant.Ghost,
            effectiveSize
        );
    }
}
