using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A horizontal bar for the top of a content area, rendered as a <c>&lt;header&gt;</c> element.
///     Typically placed at the top of <c>&lt;sa-sidebar-inset&gt;</c> and composed with
///     <c>&lt;sa-sidebar-trigger&gt;</c>, <c>&lt;sa-header-separator&gt;</c>, a breadcrumb, and
///     <c>&lt;sa-header-actions&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-header")]
public class HeaderTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "header";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
