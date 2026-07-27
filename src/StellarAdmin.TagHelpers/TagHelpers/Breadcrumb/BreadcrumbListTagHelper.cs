using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The ordered list of breadcrumb items, rendered as an <c>&lt;ol&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-breadcrumb-list")]
public class BreadcrumbListTagHelper : StellarAdminTagHelperBase
{
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
