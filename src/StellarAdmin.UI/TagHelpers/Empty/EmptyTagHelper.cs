using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     An empty-state container that communicates the absence of content, composed of a
///     header, media, title, description, and content subcomponents.
/// </summary>
[HtmlTargetElement("sa-empty")]
public class EmptyTagHelper : StellarAdminTagHelperBase
{
    public EmptyTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "empty");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-empty"),
                "flex w-full min-w-0 flex-1 flex-col items-center justify-center text-center text-balance",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
