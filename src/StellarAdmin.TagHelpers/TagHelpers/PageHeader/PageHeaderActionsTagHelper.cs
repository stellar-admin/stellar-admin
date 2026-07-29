using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     An action area inside <c>&lt;sa-page-header&gt;</c> for content such as buttons. Aligned
///     to the end of the title row on regular viewports and placed below the title and
///     description on narrow viewports.
/// </summary>
[HtmlTargetElement("sa-page-header-actions")]
public class PageHeaderActionsTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "page-header-actions");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-page-header-actions", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
