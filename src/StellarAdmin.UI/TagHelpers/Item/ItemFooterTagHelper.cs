using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the footer of an item, across its full width below the content.
/// </summary>
[HtmlTargetElement("sa-item-footer")]
public class ItemFooterTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "item-footer");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-item-footer", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
