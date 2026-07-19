using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The header region of an empty state; typically contains the media, title, and description.
/// </summary>
[HtmlTargetElement("sa-empty-header")]
public class EmptyHeaderTagHelper : StellarAdminTagHelperBase
{
    public EmptyHeaderTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "empty-header");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-empty-header"),
                "flex max-w-sm flex-col items-center",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
