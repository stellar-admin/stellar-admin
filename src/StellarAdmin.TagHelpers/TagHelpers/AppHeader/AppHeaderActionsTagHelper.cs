using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A right-aligned area at the end of <c>&lt;sa-app-header&gt;</c> for trailing content such
///     as buttons or an avatar.
/// </summary>
[HtmlTargetElement("sa-app-header-actions")]
public class AppHeaderActionsTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "app-header-actions");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-app-header-actions", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
