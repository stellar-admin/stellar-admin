using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Displays a single keyboard key or keystroke.
/// </summary>
[HtmlTargetElement("sa-kbd")]
public class KbdTagHelper : StellarAdminTagHelperBase
{
    public KbdTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "kbd";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "kbd");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(new ThemeToken("sa-kbd"), output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
