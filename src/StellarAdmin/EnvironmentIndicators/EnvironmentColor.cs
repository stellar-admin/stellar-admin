using StellarAdmin.Diagnostics;

namespace StellarAdmin;

/// <summary>
///     Represents the colour of an environment indicator.
/// </summary>
/// <remarks>
///     A colour is one of the theme's colours, which follow light and dark mode, or a custom colour
///     created with <see cref="Custom(string)" />. The text on the indicator is white or dark,
///     whichever reads better on the colour.
/// </remarks>
public abstract class EnvironmentColor
{
    /// <summary>
    ///     Gets the theme's danger colour, for an environment where changes matter most, such as
    ///     production.
    /// </summary>
    public static EnvironmentColor Danger { get; } =
        new ThemeEnvironmentColor(EnvironmentThemeColor.Danger);

    /// <summary>
    ///     Gets the theme's information colour.
    /// </summary>
    public static EnvironmentColor Info { get; } =
        new ThemeEnvironmentColor(EnvironmentThemeColor.Info);

    /// <summary>
    ///     Gets a grey colour, for an environment that needs no warning.
    /// </summary>
    public static EnvironmentColor Neutral { get; } =
        new ThemeEnvironmentColor(EnvironmentThemeColor.Neutral);

    /// <summary>
    ///     Gets the theme's success colour.
    /// </summary>
    public static EnvironmentColor Success { get; } =
        new ThemeEnvironmentColor(EnvironmentThemeColor.Success);

    /// <summary>
    ///     Gets the theme's warning colour.
    /// </summary>
    public static EnvironmentColor Warning { get; } =
        new ThemeEnvironmentColor(EnvironmentThemeColor.Warning);

    // Why the colour can't be used, when a factory method got a blank colour
    internal string? Problem { get; init; }

    private protected EnvironmentColor() { }

    /// <summary>
    ///     Returns a custom colour, used in light and dark mode.
    /// </summary>
    /// <param name="color">A CSS colour, such as <c>#7c3aed</c>.</param>
    /// <returns>The custom colour.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="color" /> is <see langword="null" />.
    /// </exception>
    public static CustomEnvironmentColor Custom(string color)
    {
        ArgumentNullException.ThrowIfNull(color);

        return new(color, null) { Problem = ConfigurationValues.FindBlank((nameof(color), color)) };
    }

    /// <summary>
    ///     Returns a custom colour, with another colour in dark mode.
    /// </summary>
    /// <param name="color">A CSS colour, such as <c>#7c3aed</c>.</param>
    /// <param name="darkColor">The CSS colour used in dark mode.</param>
    /// <returns>The custom colour.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="color" /> or <paramref name="darkColor" /> is <see langword="null" />.
    /// </exception>
    public static CustomEnvironmentColor Custom(string color, string darkColor)
    {
        ArgumentNullException.ThrowIfNull(color);
        ArgumentNullException.ThrowIfNull(darkColor);

        return new(color, darkColor)
        {
            Problem = ConfigurationValues.FindBlank(
                (nameof(color), color),
                (nameof(darkColor), darkColor)
            ),
        };
    }
}
