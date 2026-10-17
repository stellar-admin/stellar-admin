using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a horizontal line between items in a group.
/// </summary>
[HtmlTargetElement("sa-item-separator")]
public sealed class ItemSeparatorTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("data-slot", "item-separator");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-item-separator", output.GetUserSuppliedClass())
        );

        var separatorTagHelper = new SeparatorTagHelper()
        {
            Orientation = SeparatorOrientation.Horizontal,
        };
        await separatorTagHelper.ProcessAsync(context, output);
    }
}
