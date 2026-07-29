using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A header section at the top of a page's content, rendered as a <c>&lt;header&gt;</c>
///     element. Composed with an optional <c>&lt;sa-breadcrumb&gt;</c>,
///     <c>&lt;sa-page-header-title&gt;</c>, <c>&lt;sa-page-header-description&gt;</c>,
///     <c>&lt;sa-page-header-actions&gt;</c>, and <c>&lt;sa-page-header-nav&gt;</c> — in that
///     order, which is also the top-to-bottom order on narrow viewports.
/// </summary>
[HtmlTargetElement("sa-page-header")]
public class PageHeaderTagHelper : StellarAdminTagHelperBase
{
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
