using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the title of a page header as the page's <c>&lt;h1&gt;</c> heading.
/// </summary>
/// <remarks>
///     Inline content, such as an <c>&lt;sa-badge&gt;</c>, can follow the title text.
/// </remarks>
[HtmlTargetElement("sa-page-header-title")]
public class PageHeaderTitleTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "h1";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "page-header-title");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-page-header-title", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
