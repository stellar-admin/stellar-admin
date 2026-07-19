using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The main content region of a card.
/// </summary>
[HtmlTargetElement("sa-card-content")]
public class CardContentTagHelper : StellarAdminTagHelperBase
{
    public CardContentTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "card-content");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(new ThemeToken("sa-card-content"), output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
