using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Groups a set of related fields together, arranging them in a column with consistent spacing.
/// </summary>
[HtmlTargetElement("sa-field-group")]
public class FieldGroupTagHelper : StellarAdminTagHelperBase
{
    public FieldGroupTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (!output.Attributes.ContainsName("data-slot"))
        {
            output.Attributes.SetAttribute("data-slot", "field-group");
        }

        output.Attributes.SetAttribute(
            "class",
            BuildClassString(
                new ThemeToken("sa-field-group"),
                "group/field-group @container/field-group flex w-full flex-col",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
