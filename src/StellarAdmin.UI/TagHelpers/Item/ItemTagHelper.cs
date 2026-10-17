using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a row that combines media, a title, a description and actions.
/// </summary>
[HtmlTargetElement("sa-item")]
public class ItemTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the size of the item, which sets its padding and spacing.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size of the item. The default is
    ///     <see cref="ItemSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public ItemSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the item.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the visual style. The default is
    ///     <see cref="ItemVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public ItemVariant? Variant { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        ItemRenderingHelper.Render(output, Size, Variant);

        return Task.CompletedTask;
    }
}
