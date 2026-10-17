using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a list of related items.
/// </summary>
[HtmlTargetElement("sa-item-group")]
public sealed class ItemGroupTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "list");
        output.Attributes.SetAttribute("data-slot", "item-group");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-item-group", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
