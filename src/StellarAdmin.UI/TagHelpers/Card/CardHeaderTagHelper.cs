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
                "group/card-header",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
