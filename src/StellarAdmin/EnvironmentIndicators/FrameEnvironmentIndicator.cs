namespace StellarAdmin;

/// <summary>
///     Represents an indicator that shows a border around the window, with the name of the environment on a tab at the top.
/// </summary>
/// <remarks>
///     Create a frame with <see cref="EnvironmentIndicator.Frame(string)" />.
/// </remarks>
public sealed class FrameEnvironmentIndicator : EnvironmentIndicator
{
    /// <summary>
    ///     Gets or sets the size of the tab that names the environment.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="EnvironmentIndicatorTabSize.Medium" />.
    /// </value>
    /// <remarks>
    ///     A value that isn't one of the enumeration values is a configuration problem, and the
    ///     default is used.
    /// </remarks>
    public EnvironmentIndicatorTabSize TabSize { get; set; } = EnvironmentIndicatorTabSize.Medium;

    internal FrameEnvironmentIndicator(string label)
        : base(label) { }
}
