using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A secondary line of muted text within a card header, describing the card's contents.
/// </summary>
[HtmlTargetElement("sa-card-description")]
public class CardDescriptionTagHelper : StellarAdminTagHelperBase
{
    public CardDescriptionTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "card-description");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(new ThemeToken("sa-card-description"), output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
