using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Displays a single keyboard key or keystroke.
/// </summary>
[HtmlTargetElement("sa-kbd")]
public class KbdTagHelper : StellarAdminTagHelperBase
{
    public KbdTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "kbd";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "kbd");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-kbd"),
                "pointer-events-none inline-flex items-center justify-center select-none",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
