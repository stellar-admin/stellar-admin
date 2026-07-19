using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The ordered list of breadcrumb items, rendered as an <c>&lt;ol&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-breadcrumb-list")]
public class BreadcrumbListTagHelper : StellarAdminTagHelperBase
{
    public BreadcrumbListTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "ol";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "breadcrumb-list");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(
                new ThemeToken("sa-breadcrumb-list"),
                "flex flex-wrap items-center break-words",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
