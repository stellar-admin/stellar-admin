using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The application's top navigation bar, rendered as a <c>&lt;header&gt;</c> element.
///     Typically placed at the top of <c>&lt;sa-sidebar-inset&gt;</c> and composed with
///     <c>&lt;sa-sidebar-trigger&gt;</c>, <c>&lt;sa-app-header-separator&gt;</c>, a breadcrumb,
///     and <c>&lt;sa-app-header-actions&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-app-header")]
public class AppHeaderTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "header";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "app-header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-app-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
