using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A full-width navigation area at the bottom of <c>&lt;sa-page-header&gt;</c>, rendered as
///     a <c>&lt;nav&gt;</c> element. It positions whatever navigation the page provides —
///     typically an <c>&lt;sa-tab-list&gt;</c> — without imposing any styling of its own.
/// </summary>
[HtmlTargetElement("sa-page-header-nav")]
public class PageHeaderNavTagHelper : StellarAdminTagHelperBase
{
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
