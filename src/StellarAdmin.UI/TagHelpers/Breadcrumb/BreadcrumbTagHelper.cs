using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a breadcrumb that shows the path to the current page.
/// </summary>
[HtmlTargetElement("sa-breadcrumb")]
public class BreadcrumbTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "nav";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("aria-label", "breadcrumb");
        output.Attributes.SetAttribute("data-slot", "breadcrumb");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-breadcrumb", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
