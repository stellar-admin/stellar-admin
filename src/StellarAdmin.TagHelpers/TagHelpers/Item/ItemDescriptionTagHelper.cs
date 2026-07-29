using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The secondary descriptive text of an item, rendered beneath its title.
/// </summary>
[HtmlTargetElement("sa-item-description")]
public class ItemDescriptionTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "p";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "item-description");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-item-description", GetUserSpecifiedClass(output))
        );

        return Task.CompletedTask;
    }
}
