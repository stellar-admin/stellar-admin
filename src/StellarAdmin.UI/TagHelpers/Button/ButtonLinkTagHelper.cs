using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a link that looks like a button.
/// </summary>
/// <remarks>
///     Set <c>href</c> or the <c>asp-*</c> routing attributes.
/// </remarks>
[HtmlTargetElement("sa-button-link")]
public sealed class ButtonLinkTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ButtonLinkTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">
    ///     The generator that builds the link's URL from the routing attributes.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public ButtonLinkTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

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
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="ButtonVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public ButtonVariant? Variant { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveSize = Size ?? ButtonSize.Default;
        var effectiveVariant = Variant ?? ButtonVariant.Default;

        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        await ApplyRouteAttributesAsync(_htmlGenerator, context, output);

        ButtonRenderingHelper.RenderAttributes(output, effectiveVariant, effectiveSize);
    }
}
