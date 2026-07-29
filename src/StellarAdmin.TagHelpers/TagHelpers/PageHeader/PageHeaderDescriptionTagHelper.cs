using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A short description below the title of <c>&lt;sa-page-header&gt;</c>, rendered as a
///     <c>&lt;p&gt;</c> element.
/// </summary>
[HtmlTargetElement("sa-page-header-description")]
public class PageHeaderDescriptionTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "p";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "page-header-description");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-page-header-description", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
