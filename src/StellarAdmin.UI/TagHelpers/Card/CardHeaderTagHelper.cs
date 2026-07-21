using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The header region of a card; typically contains the title, description, and action.
/// </summary>
[HtmlTargetElement("sa-card-header")]
public class CardHeaderTagHelper : StellarAdminTagHelperBase
{
    public CardHeaderTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "card-header");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(
                new ThemeToken("sa-card-header"),
                "group/card-header @container/card-header grid auto-rows-min items-start has-data-[slot=card-action]:grid-cols-[1fr_auto] has-data-[slot=card-description]:grid-rows-[auto_auto]",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
