using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an area at the end of an app header for content such as buttons or an avatar.
/// </summary>
[HtmlTargetElement("sa-app-header-actions")]
public class AppHeaderActionsTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
