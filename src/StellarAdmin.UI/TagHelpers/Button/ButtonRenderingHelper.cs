using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

internal static class ButtonRenderingHelper
{
    private static readonly Dictionary<ButtonVariant, string> ButtonVariantClasses = new Dictionary<
        ButtonVariant,
        string
    >
    {
        [ButtonVariant.Default] = "sa-button-variant-default",
        [ButtonVariant.Destructive] = "sa-button-variant-destructive",
        [ButtonVariant.Outline] = "sa-button-variant-outline",
        [ButtonVariant.Secondary] = "sa-button-variant-secondary",
        [ButtonVariant.Ghost] = "sa-button-variant-ghost",
        [ButtonVariant.Link] = "sa-button-variant-link",
    };

    private static readonly Dictionary<ButtonSize, string> ButtonSizeClasses = new Dictionary<
        ButtonSize,
        string
    >
    {
        [ButtonSize.Default] = "sa-button-size-default",
        [ButtonSize.ExtraSmall] = "sa-button-size-xs",
        [ButtonSize.Small] = "sa-button-size-sm",
        [ButtonSize.Large] = "sa-button-size-lg",
        [ButtonSize.Icon] = "sa-button-size-icon",
        [ButtonSize.IconExtraSmall] = "sa-button-size-icon-xs",
        [ButtonSize.IconSmall] = "sa-button-size-icon-sm",
        [ButtonSize.IconLarge] = "sa-button-size-icon-lg",
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
                "sa-button",
                "group/button",
                ButtonVariantClasses[variant],
                ButtonSizeClasses[size],
                output.GetUserSuppliedClass()
            )
        );
    }
}
