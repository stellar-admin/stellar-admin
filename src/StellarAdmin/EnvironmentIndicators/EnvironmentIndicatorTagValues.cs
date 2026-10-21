using StellarAdmin.UI;

namespace StellarAdmin;

// Maps an indicator's settings onto the attributes of the StellarAdmin.UI tag that renders it
internal static class EnvironmentIndicatorTagValues
{
    public static string? GetCustomColor(EnvironmentColor color) =>
        (color as CustomEnvironmentColor)?.Color;

    public static string? GetCustomDarkColor(EnvironmentColor color) =>
        (color as CustomEnvironmentColor)?.DarkColor;

    public static EnvironmentBannerColor ToBannerColor(EnvironmentColor color) =>
        GetThemeColor(color) switch
        {
            EnvironmentThemeColor.Info => EnvironmentBannerColor.Info,
            EnvironmentThemeColor.Success => EnvironmentBannerColor.Success,
            EnvironmentThemeColor.Warning => EnvironmentBannerColor.Warning,
            EnvironmentThemeColor.Danger => EnvironmentBannerColor.Danger,
            _ => EnvironmentBannerColor.Neutral,
        };

    public static EnvironmentFrameColor ToFrameColor(EnvironmentColor color) =>
        GetThemeColor(color) switch
        {
            EnvironmentThemeColor.Info => EnvironmentFrameColor.Info,
            EnvironmentThemeColor.Success => EnvironmentFrameColor.Success,
            EnvironmentThemeColor.Warning => EnvironmentFrameColor.Warning,
            EnvironmentThemeColor.Danger => EnvironmentFrameColor.Danger,
            _ => EnvironmentFrameColor.Neutral,
        };

    public static EnvironmentFrameTabSize ToFrameTabSize(EnvironmentIndicatorTabSize size) =>
        size switch
        {
            EnvironmentIndicatorTabSize.Small => EnvironmentFrameTabSize.Small,
            EnvironmentIndicatorTabSize.Large => EnvironmentFrameTabSize.Large,
            EnvironmentIndicatorTabSize.ExtraLarge => EnvironmentFrameTabSize.ExtraLarge,
            _ => EnvironmentFrameTabSize.Medium,
        };

    public static EnvironmentStripeColor ToStripeColor(EnvironmentColor color) =>
        GetThemeColor(color) switch
        {
            EnvironmentThemeColor.Info => EnvironmentStripeColor.Info,
            EnvironmentThemeColor.Success => EnvironmentStripeColor.Success,
            EnvironmentThemeColor.Warning => EnvironmentStripeColor.Warning,
            EnvironmentThemeColor.Danger => EnvironmentStripeColor.Danger,
            _ => EnvironmentStripeColor.Neutral,
        };

    public static EnvironmentStripeTabSize ToStripeTabSize(EnvironmentIndicatorTabSize size) =>
        size switch
        {
            EnvironmentIndicatorTabSize.Small => EnvironmentStripeTabSize.Small,
            EnvironmentIndicatorTabSize.Large => EnvironmentStripeTabSize.Large,
            EnvironmentIndicatorTabSize.ExtraLarge => EnvironmentStripeTabSize.ExtraLarge,
            _ => EnvironmentStripeTabSize.Medium,
        };

    // A custom colour is rendered through the custom colour attributes, which override the theme
    // colour
    private static EnvironmentThemeColor GetThemeColor(EnvironmentColor color) =>
        (color as ThemeEnvironmentColor)?.Value ?? EnvironmentThemeColor.Neutral;
}
