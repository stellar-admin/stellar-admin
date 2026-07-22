using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The current page in the breadcrumb trail, rendered as a non-interactive
///     <c>&lt;span&gt;</c> marked with <c>aria-current="page"</c>.
/// </summary>
[HtmlTargetElement("sa-breadcrumb-page")]
public class BreadcrumbPageTagHelper : StellarAdminTagHelperBase
{
    public BreadcrumbPageTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "breadcrumb-page");
        output.Attributes.SetAttribute("role", "link");
        output.Attributes.SetAttribute("aria-disabled", "true");
        output.Attributes.SetAttribute("aria-current", "page");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString("sa-breadcrumb-page", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
