using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the area of a page header that holds actions, such as buttons.
/// </summary>
/// <remarks>
///     The actions sit at the end of the title row, or below the title and description on narrow
///     screens.
/// </remarks>
[HtmlTargetElement("sa-page-header-actions")]
public sealed class PageHeaderActionsTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
