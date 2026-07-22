using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

internal static class ItemRenderingHelper
{
    private static readonly Dictionary<ItemSize, string> ItemSizeClasses = new()
    {
        [ItemSize.Default] = "sa-item-size-default",
        [ItemSize.Small] = "sa-item-size-sm",
        [ItemSize.ExtraSmall] = "sa-item-size-xs",
    };

    private static readonly Dictionary<ItemVariant, string> ItemVariantClasses = new()
    {
        [ItemVariant.Default] = "sa-item-variant-default",
        [ItemVariant.Outline] = "sa-item-variant-outline",
        [ItemVariant.Muted] = "sa-item-variant-muted",
    };

    public static async Task RenderAsync(
        TagHelperOutput output,
        ICssClassMerger classMerger,
        ItemSize? size,
        ItemVariant? variant
    )
    {
        var effectiveSize = size ?? ItemSize.Default;
        var effectiveVariant = variant ?? ItemVariant.Default;

        output.Attributes.SetAttribute("data-slot", "item");
        output.Attributes.SetAttribute("data-variant", effectiveVariant.GetDataAttributeText());
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());

        output.Attributes.SetAttribute(
            "class",
            classMerger.Merge(
                "sa-item",
                "group/item",
                ItemSizeClasses[effectiveSize],
                ItemVariantClasses[effectiveVariant],
                output.GetUserSuppliedClass()
            )
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
