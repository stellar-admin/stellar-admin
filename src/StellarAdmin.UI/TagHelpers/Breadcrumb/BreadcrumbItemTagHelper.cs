using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an item in a breadcrumb list, which holds a link or the current page.
/// </summary>
[HtmlTargetElement("sa-breadcrumb-item")]
public sealed class BreadcrumbItemTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "li";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "breadcrumb-item");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-breadcrumb-item", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
