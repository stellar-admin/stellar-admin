namespace StellarAdmin;

/// <summary>
///     Represents a custom colour of an environment indicator.
/// </summary>
/// <remarks>
///     Create a custom colour with <see cref="EnvironmentColor.Custom(string)" /> or
///     <see cref="EnvironmentColor.Custom(string, string)" />.
/// </remarks>
public sealed class CustomEnvironmentColor : EnvironmentColor
{
    /// <summary>
    ///     Gets the CSS colour.
    /// </summary>
    public string Color { get; }

    /// <summary>
    ///     Gets the CSS colour used in dark mode.
    /// </summary>
    /// <value>
    ///     The dark mode colour, or <see langword="null" /> to use <see cref="Color" /> in dark
    ///     mode too.
    /// </value>
    public string? DarkColor { get; }

    internal CustomEnvironmentColor(string color, string? darkColor)
    {
        Color = color;
        DarkColor = darkColor;
    }
}
