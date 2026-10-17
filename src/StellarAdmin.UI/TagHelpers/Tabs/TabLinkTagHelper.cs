using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a tab in a <c>&lt;sa-tab-list&gt;</c> as a link to another page.
/// </summary>
[HtmlTargetElement("sa-tab-link")]
public class TabLinkTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Gets or sets a value that indicates whether the tab is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the tab is disabled and doesn't link anywhere; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the tab is the selected tab.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the tab is selected; otherwise, <see langword="false" />. By
    ///     default, the tab is selected when its page or action routing attributes match the
    ///     current request.
    /// </value>
    [HtmlAttributeName("active")]
    public bool? Active { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TabLinkTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">
    ///     The generator that resolves the link's <c>href</c> from its routing attributes.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public TabLinkTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveDisabled = Disabled ?? false;

        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        await ApplyRouteAttributesAsync(_htmlGenerator, context, output);

        var isActive = Active ?? IsActiveRoute();
        output.Attributes.SetAttribute("role", "tab");
        output.Attributes.SetAttribute("data-slot", "tabs-trigger");
        output.Attributes.SetAttribute("aria-selected", isActive ? "true" : "false");
        if (isActive)
        {
            output.Attributes.SetAttribute("data-active", "");
        }

        output.Attributes.SetAttribute("aria-disabled", effectiveDisabled ? "true" : "false");
        if (effectiveDisabled)
        {
            if (output.Attributes.TryGetAttribute("href", out var hrefAttribute))
            {
                output.Attributes.Remove(hrefAttribute);
            }
            output.Attributes.SetAttribute("data-disabled", "");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-tabs-trigger", output.GetUserSuppliedClass())
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
