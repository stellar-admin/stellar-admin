namespace StellarAdmin.UI;

/// <summary>
///     Specifies the colour of a <c>&lt;sa-environment-banner&gt;</c>.
/// </summary>
public enum EnvironmentBannerColor
{
    /// <summary>
    ///     A grey colour, for an environment that needs no warning.
    /// </summary>
    Neutral,

    /// <summary>
    ///     The theme's information colour.
    /// </summary>
    Info,

    /// <summary>
    ///     The theme's success colour.
    /// </summary>
    Success,

    /// <summary>
    ///     The theme's warning colour.
    /// </summary>
    Warning,

    /// <summary>
    ///     The theme's danger colour, for an environment where changes matter most, such as production.
    /// </summary>
    Danger,
}

internal static class EnvironmentBannerColorExtensions
{
    extension(EnvironmentBannerColor color)
    {
        public string GetDataAttributeText() =>
            color switch
            {
                EnvironmentBannerColor.Neutral => "neutral",
                EnvironmentBannerColor.Info => "info",
                EnvironmentBannerColor.Success => "success",
                EnvironmentBannerColor.Warning => "warning",
                EnvironmentBannerColor.Danger => "danger",
                _ => string.Empty,
            };
    }
}
