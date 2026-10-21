namespace StellarAdmin.UI;

/// <summary>
///     Specifies the colour of a <c>&lt;sa-environment-frame&gt;</c>.
/// </summary>
public enum EnvironmentFrameColor
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

internal static class EnvironmentFrameColorExtensions
{
    extension(EnvironmentFrameColor color)
    {
        public string GetDataAttributeText() =>
            color switch
            {
                EnvironmentFrameColor.Neutral => "neutral",
                EnvironmentFrameColor.Info => "info",
                EnvironmentFrameColor.Success => "success",
                EnvironmentFrameColor.Warning => "warning",
                EnvironmentFrameColor.Danger => "danger",
                _ => string.Empty,
            };
    }
}
