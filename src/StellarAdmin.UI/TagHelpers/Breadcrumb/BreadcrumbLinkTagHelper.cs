using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a link in a breadcrumb item.
/// </summary>
/// <remarks>
///     Set <c>href</c> or the <c>asp-*</c> routing attributes.
/// </remarks>
[HtmlTargetElement("sa-breadcrumb-link")]
public class BreadcrumbLinkTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BreadcrumbLinkTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">
    ///     The generator that builds the link's URL from the routing attributes.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public BreadcrumbLinkTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        await ApplyRouteAttributesAsync(_htmlGenerator, context, output);

        output.Attributes.SetAttribute("data-slot", "breadcrumb-link");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-breadcrumb-link", output.GetUserSuppliedClass())
        );
    }
}
