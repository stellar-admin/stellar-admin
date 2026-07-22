using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The leading media region of an item, holding an icon, image, or avatar.
/// </summary>
[HtmlTargetElement("sa-item-media")]
public class ItemMediaTagHelper : StellarAdminTagHelperBase
{
    private static readonly Dictionary<ItemMediaVariant, string> ItemVariantClasses =
        new Dictionary<ItemMediaVariant, string>
        {
            [ItemMediaVariant.Default] = "sa-item-media-variant-default",
            [ItemMediaVariant.Icon] = "sa-item-media-variant-icon",
            [ItemMediaVariant.Image] = "sa-item-media-variant-image",
        };

    /// <summary>
    ///     The kind of media the region contains, which controls its sizing and styling.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ItemMediaVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public ItemMediaVariant? Variant { get; set; }

    public ItemMediaTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? ItemMediaVariant.Default;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "item-media");
        output.Attributes.SetAttribute("data-variant", effectiveVariant.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                "sa-item-media",
                ItemVariantClasses[effectiveVariant],
                GetUserSpecifiedClass(output)
            )
        );

        return Task.CompletedTask;
    }
}
