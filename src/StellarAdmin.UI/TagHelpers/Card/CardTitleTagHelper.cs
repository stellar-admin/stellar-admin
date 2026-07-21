using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The title text within a card header.
/// </summary>
[HtmlTargetElement("sa-card-title")]
public class CardTitleTagHelper : StellarAdminTagHelperBase
{
    public CardTitleTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "card-title");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(new ThemeToken("sa-card-title"), output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
