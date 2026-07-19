using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The content region of an empty state; typically contains actions or supplementary
///     elements below the header.
/// </summary>
[HtmlTargetElement("sa-empty-content")]
public class EmptyContentTagHelper : StellarAdminTagHelperBase
{
    public EmptyContentTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "empty-content");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-empty-content"),
                "flex w-full max-w-sm min-w-0 flex-col items-center text-balance",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
