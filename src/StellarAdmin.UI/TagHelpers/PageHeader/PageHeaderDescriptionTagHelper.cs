using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a short description below the title of a page header.
/// </summary>
[HtmlTargetElement("sa-page-header-description")]
public sealed class PageHeaderDescriptionTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
