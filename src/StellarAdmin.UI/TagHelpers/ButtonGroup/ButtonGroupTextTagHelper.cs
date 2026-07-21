using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Renders a non-interactive text label within a button group.
/// </summary>
[HtmlTargetElement("sa-button-group-text")]
public class ButtonGroupTextTagHelper : StellarAdminTagHelperBase
{
    public ButtonGroupTextTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "button-group-text");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(
                new ThemeToken("sa-button-group-text"),
                "flex items-center [&_svg]:pointer-events-none",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
