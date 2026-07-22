using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The footer region of a card; typically contains actions or supplementary information.
/// </summary>
[HtmlTargetElement("sa-card-footer")]
public class CardFooterTagHelper : StellarAdminTagHelperBase
{
    public CardFooterTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "card-footer");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString("sa-card-footer", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
