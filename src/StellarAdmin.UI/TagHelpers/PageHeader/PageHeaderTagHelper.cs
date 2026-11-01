using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a header at the top of a page's content.
/// </summary>
/// <remarks>
///     Place an optional <c>&lt;sa-breadcrumb&gt;</c>, <c>&lt;sa-page-header-title&gt;</c>,
///     <c>&lt;sa-page-header-description&gt;</c>, <c>&lt;sa-page-header-actions&gt;</c> and
///     <c>&lt;sa-page-header-nav&gt;</c> inside, in that order. Narrow screens stack them in the
///     same order.
/// </remarks>
[HtmlTargetElement("sa-page-header")]
public sealed class PageHeaderTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets a value that indicates whether the header stays at the top of the page as it
    ///     scrolls.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to keep the header in view, ruled off from the content that
    ///     scrolls under it; otherwise, <see langword="false" />. The default is
    ///     <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Directly inside an <c>&lt;sa-page-container&gt;</c>, the header reaches the container's
    ///     edges. On a narrow screen it hides its breadcrumb and description.
    /// </remarks>
    [HtmlAttributeName("sticky")]
    public bool Sticky { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "header";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "page-header");

        if (Sticky)
        {
            output.Attributes.SetAttribute("data-sticky", "");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-page-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
