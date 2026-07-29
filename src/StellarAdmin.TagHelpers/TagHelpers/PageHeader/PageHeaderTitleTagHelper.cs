using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The page title inside <c>&lt;sa-page-header&gt;</c>, rendered as the page's
///     <c>&lt;h1&gt;</c> element. Inline trailing content such as an <c>&lt;sa-badge&gt;</c>
///     can be placed directly after the title text.
/// </summary>
[HtmlTargetElement("sa-page-header-title")]
public class PageHeaderTitleTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "h1";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "page-header-title");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-page-header-title",
                "sa-font-heading",
                "font-heading",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
