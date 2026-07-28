using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A right-aligned area at the end of <c>&lt;sa-header&gt;</c> for trailing content such as
///     buttons or an avatar.
/// </summary>
[HtmlTargetElement("sa-header-actions")]
public class HeaderActionsTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "header-actions");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-header-actions", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
