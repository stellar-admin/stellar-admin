using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

internal static class ItemRenderingHelper
{
    private static readonly Dictionary<ItemSize, ThemeToken> ItemSizeClasses = new()
    {
        [ItemSize.Default] = new ThemeToken("sa-item-size-default"),
        [ItemSize.Small] = new ThemeToken("sa-item-size-sm"),
        [ItemSize.ExtraSmall] = new ThemeToken("sa-item-size-xs"),
    };

    private static readonly Dictionary<ItemVariant, ThemeToken> ItemVariantClasses = new()
    {
        [ItemVariant.Default] = new ThemeToken("sa-item-variant-default"),
        [ItemVariant.Outline] = new ThemeToken("sa-item-variant-outline"),
        [ItemVariant.Muted] = new ThemeToken("sa-item-variant-muted"),
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
                new ThemeToken("sa-item"),
                "w-full group/item focus-visible:border-ring focus-visible:ring-ring/50 flex items-center flex-wrap outline-none transition-colors duration-100 focus-visible:ring-[3px] [a]:transition-colors",
                ItemSizeClasses[effectiveSize],
                ItemVariantClasses[effectiveVariant],
                output.GetUserSuppliedClass()
            )
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
