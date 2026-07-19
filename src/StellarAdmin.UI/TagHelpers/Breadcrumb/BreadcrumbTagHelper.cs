using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A breadcrumb navigation trail, rendered as a <c>&lt;nav&gt;</c>; shows the path to the
///     current page. Compose it with the list, item, link, page, separator, and ellipsis
///     subcomponents.
/// </summary>
[HtmlTargetElement("sa-breadcrumb")]
public class BreadcrumbTagHelper : StellarAdminTagHelperBase
{
    public BreadcrumbTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "nav";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("aria-label", "breadcrumb");
        output.Attributes.SetAttribute("data-slot", "breadcrumb");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(new ThemeToken("sa-breadcrumb"), output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
