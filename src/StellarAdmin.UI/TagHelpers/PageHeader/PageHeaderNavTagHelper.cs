using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a full-width navigation area at the bottom of a page header.
/// </summary>
/// <remarks>
///     The area places the page's navigation, such as an <c>&lt;sa-tab-list&gt;</c>, without
///     styling it.
/// </remarks>
[HtmlTargetElement("sa-page-header-nav")]
public sealed class PageHeaderNavTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "nav";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "page-header-nav");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-page-header-nav", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
