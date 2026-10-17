using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the ordered list of items in a breadcrumb.
/// </summary>
[HtmlTargetElement("sa-breadcrumb-list")]
public sealed class BreadcrumbListTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "ol";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "breadcrumb-list");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-breadcrumb-list", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
