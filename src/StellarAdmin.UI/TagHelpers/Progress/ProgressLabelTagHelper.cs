using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A text label for a progress bar, rendered as a <c>&lt;span&gt;</c>.
/// </summary>
[HtmlTargetElement("sa-progress-label")]
public class ProgressLabelTagHelper : StellarAdminTagHelperBase
{
    public ProgressLabelTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "progress-label");
        output.Attributes.Add(
            "class",
            ClassMerger.Merge(new ThemeToken("sa-progress-label"), output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
