using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     An action region within a card header, aligned to the top-right corner; typically
///     contains a button or other interactive control.
/// </summary>
[HtmlTargetElement("sa-card-action")]
public class CardActionTagHelper : StellarAdminTagHelperBase
{
    public CardActionTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "card-action");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(
                new ThemeToken("sa-card-action"),
                "col-start-2 row-span-2 row-start-1 self-start justify-self-end",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
