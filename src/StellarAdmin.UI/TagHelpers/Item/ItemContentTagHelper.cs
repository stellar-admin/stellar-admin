using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the main content of an item, which usually holds its title and description.
/// </summary>
[HtmlTargetElement("sa-item-content")]
public class ItemContentTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "item-content");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-item-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
