using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A single item within the breadcrumb trail, rendered as a <c>&lt;li&gt;</c>; wraps a
///     link, page, or separator.
/// </summary>
[HtmlTargetElement("sa-breadcrumb-item")]
public class BreadcrumbItemTagHelper : StellarAdminTagHelperBase
{
    public BreadcrumbItemTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "li";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "breadcrumb-item");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(new ThemeToken("sa-breadcrumb-item"), output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
