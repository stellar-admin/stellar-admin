namespace StellarAdmin;

/// <summary>
///     Represents an indicator that shows a bar across the top of the window.
/// </summary>
/// <remarks>
///     Create a banner with <see cref="EnvironmentIndicator.Banner(string)" />.
/// </remarks>
public sealed class BannerEnvironmentIndicator : EnvironmentIndicator
{
    /// <summary>
    ///     Gets or sets the text shown after the name of the environment.
    /// </summary>
    /// <value>
    ///     The description, or <see langword="null" /> for none. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     An empty or white-space value shows no description.
    /// </remarks>
    public string? Description
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    internal BannerEnvironmentIndicator(string label)
        : base(label) { }
}
