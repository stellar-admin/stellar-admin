using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The header region of a card; typically contains the title, description, and action.
/// </summary>
[HtmlTargetElement("sa-card-header")]
public class CardHeaderTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "card-header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-card-header", "group/card-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
