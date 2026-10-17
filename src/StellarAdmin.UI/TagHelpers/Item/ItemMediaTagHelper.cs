using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the media of an item, such as an icon, image or avatar, at its start.
/// </summary>
[HtmlTargetElement("sa-item-media")]
public sealed class ItemMediaTagHelper : StellarAdminTagHelperBase
{
    private static readonly Dictionary<ItemMediaVariant, string> ItemVariantClasses =
        new Dictionary<ItemMediaVariant, string>
        {
            [ItemMediaVariant.Default] = "sa-item-media-variant-default",
            [ItemMediaVariant.Icon] = "sa-item-media-variant-icon",
            [ItemMediaVariant.Image] = "sa-item-media-variant-image",
        };

    /// <summary>
    ///     Gets or sets the kind of media, which sets its size and style.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the kind of media. The default is
    ///     <see cref="ItemMediaVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public ItemMediaVariant? Variant { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? ItemMediaVariant.Default;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "item-media");
        output.Attributes.SetAttribute("data-variant", effectiveVariant.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-item-media",
                ItemVariantClasses[effectiveVariant],
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
