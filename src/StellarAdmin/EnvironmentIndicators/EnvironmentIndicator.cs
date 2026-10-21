using StellarAdmin.Diagnostics;

namespace StellarAdmin;

/// <summary>
///     Represents the indicator that shows which environment the application runs in.
/// </summary>
/// <remarks>
///     Create an indicator with <see cref="Stripe(string)" />, <see cref="Frame(string)" /> or
///     <see cref="Banner(string)" />.
/// </remarks>
public abstract class EnvironmentIndicator
{
    /// <summary>
    ///     Gets or sets the colour of the indicator.
    /// </summary>
    /// <value>
    ///     The colour. The default is <see cref="EnvironmentColor.Neutral" />.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public EnvironmentColor Color
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            field = value;
        }
    } = EnvironmentColor.Neutral;

    /// <summary>
    ///     Gets the name of the environment.
    /// </summary>
    public string Label { get; }

    // Why the indicator can't be shown, when a factory method got a blank label
    internal string? Problem { get; init; }

    private protected EnvironmentIndicator(string label) => Label = label;

    /// <summary>
    ///     Returns an indicator that shows a bar across the top of the window.
    /// </summary>
    /// <param name="label">The name of the environment.</param>
    /// <returns>An indicator that shows a banner.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="label" /> is <see langword="null" />.
    /// </exception>
    public static BannerEnvironmentIndicator Banner(string label)
    {
        ArgumentNullException.ThrowIfNull(label);

        return new(label) { Problem = ConfigurationValues.FindBlank((nameof(label), label)) };
    }

    /// <summary>
    ///     Returns an indicator that shows a bar across the top of the window, configured by a
    ///     delegate.
    /// </summary>
    /// <param name="label">The name of the environment.</param>
    /// <param name="configure">A delegate that configures the indicator.</param>
    /// <returns>An indicator that shows a banner.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="label" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public static BannerEnvironmentIndicator Banner(
        string label,
        Action<BannerEnvironmentIndicator> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        var indicator = Banner(label);
        configure(indicator);

        return indicator;
    }

    /// <summary>
    ///     Returns an indicator that shows a border around the window, with the name of the
    ///     environment on a tab at the top.
    /// </summary>
    /// <param name="label">The name of the environment.</param>
    /// <returns>An indicator that shows a frame.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="label" /> is <see langword="null" />.
    /// </exception>
    public static FrameEnvironmentIndicator Frame(string label)
    {
        ArgumentNullException.ThrowIfNull(label);

        return new(label) { Problem = ConfigurationValues.FindBlank((nameof(label), label)) };
    }

    /// <summary>
    ///     Returns an indicator that shows a border around the window, with the name of the
    ///     environment on a tab at the top, configured by a delegate.
    /// </summary>
    /// <param name="label">The name of the environment.</param>
    /// <param name="configure">A delegate that configures the indicator.</param>
    /// <returns>An indicator that shows a frame.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="label" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public static FrameEnvironmentIndicator Frame(
        string label,
        Action<FrameEnvironmentIndicator> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        var indicator = Frame(label);
        configure(indicator);

        return indicator;
    }

    /// <summary>
    ///     Returns an indicator that shows a thin bar across the top of the window, with the name
    ///     of the environment on a tab.
    /// </summary>
    /// <param name="label">The name of the environment.</param>
    /// <returns>An indicator that shows a stripe.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="label" /> is <see langword="null" />.
    /// </exception>
    public static StripeEnvironmentIndicator Stripe(string label)
    {
        ArgumentNullException.ThrowIfNull(label);

        return new(label) { Problem = ConfigurationValues.FindBlank((nameof(label), label)) };
    }

    /// <summary>
    ///     Returns an indicator that shows a thin bar across the top of the window, with the name
    ///     of the environment on a tab, configured by a delegate.
    /// </summary>
    /// <param name="label">The name of the environment.</param>
    /// <param name="configure">A delegate that configures the indicator.</param>
    /// <returns>An indicator that shows a stripe.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="label" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public static StripeEnvironmentIndicator Stripe(
        string label,
        Action<StripeEnvironmentIndicator> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        var indicator = Stripe(label);
        configure(indicator);

        return indicator;
    }
}
