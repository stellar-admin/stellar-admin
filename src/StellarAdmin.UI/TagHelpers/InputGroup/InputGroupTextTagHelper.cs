using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A run of text or an icon displayed inside an input group, typically within an add-on.
/// </summary>
[HtmlTargetElement("sa-input-group-text")]
public class InputGroupTextTagHelper : StellarAdminTagHelperBase
{
    public InputGroupTextTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-input-group-text"),
                "flex items-center [&_svg]:pointer-events-none",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
