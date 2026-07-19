using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A line of muted descriptive text within an empty state header.
/// </summary>
[HtmlTargetElement("sa-empty-description")]
public class EmptyDescriptionTagHelper : StellarAdminTagHelperBase
{
    public EmptyDescriptionTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "empty-description");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-empty-description"),
                "text-muted-foreground [&>a:hover]:text-primary [&>a]:underline [&>a]:underline-offset-4",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
