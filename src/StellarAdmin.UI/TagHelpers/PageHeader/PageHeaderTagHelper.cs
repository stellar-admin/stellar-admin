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
public class PageHeaderTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "header";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "page-header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-page-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
