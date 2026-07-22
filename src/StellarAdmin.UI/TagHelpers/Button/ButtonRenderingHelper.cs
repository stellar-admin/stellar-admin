using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

internal static class ButtonRenderingHelper
{
    private static readonly Dictionary<ButtonVariant, ThemeToken> ButtonVariantClasses =
        new Dictionary<ButtonVariant, ThemeToken>
        {
            [ButtonVariant.Default] = new ThemeToken("sa-button-variant-default"),
            [ButtonVariant.Destructive] = new ThemeToken("sa-button-variant-destructive"),
            [ButtonVariant.Outline] = new ThemeToken("sa-button-variant-outline"),
            [ButtonVariant.Secondary] = new ThemeToken("sa-button-variant-secondary"),
            [ButtonVariant.Ghost] = new ThemeToken("sa-button-variant-ghost"),
            [ButtonVariant.Link] = new ThemeToken("sa-button-variant-link"),
        };

    private static readonly Dictionary<ButtonSize, ThemeToken> ButtonSizeClasses = new Dictionary<
        ButtonSize,
        ThemeToken
    >
    {
        [ButtonSize.Default] = new ThemeToken("sa-button-size-default"),
        [ButtonSize.ExtraSmall] = new ThemeToken("sa-button-size-xs"),
        [ButtonSize.Small] = new ThemeToken("sa-button-size-sm"),
        [ButtonSize.Large] = new ThemeToken("sa-button-size-lg"),
        [ButtonSize.Icon] = new ThemeToken("sa-button-size-icon"),
        [ButtonSize.IconExtraSmall] = new ThemeToken("sa-button-size-icon-xs"),
        [ButtonSize.IconSmall] = new ThemeToken("sa-button-size-icon-sm"),
        [ButtonSize.IconLarge] = new ThemeToken("sa-button-size-icon-lg"),
    };

    public static void RenderAttributes(
        TagHelperOutput output,
        ICssClassMerger classMerger,
        ButtonVariant variant,
        ButtonSize size
    )
    {
        if (!output.Attributes.ContainsName("data-slot"))
        {
            output.Attributes.SetAttribute("data-slot", "button");
        }

        output.Attributes.SetAttribute(
            "class",
            classMerger.Merge(
                new ThemeToken("sa-button"),
                "group/button",
                ButtonVariantClasses[variant],
                ButtonSizeClasses[size],
                output.GetUserSuppliedClass()
            )
        );
    }
}
