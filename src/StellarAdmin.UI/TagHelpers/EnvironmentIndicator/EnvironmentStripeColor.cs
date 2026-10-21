namespace StellarAdmin.UI;

/// <summary>
///     Specifies the colour of a <c>&lt;sa-environment-stripe&gt;</c>.
/// </summary>
public enum EnvironmentStripeColor
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

internal static class EnvironmentStripeColorExtensions
{
    extension(EnvironmentStripeColor color)
    {
        public string GetDataAttributeText() =>
            color switch
            {
                EnvironmentStripeColor.Neutral => "neutral",
                EnvironmentStripeColor.Info => "info",
                EnvironmentStripeColor.Success => "success",
                EnvironmentStripeColor.Warning => "warning",
                EnvironmentStripeColor.Danger => "danger",
                _ => string.Empty,
            };
    }
}
