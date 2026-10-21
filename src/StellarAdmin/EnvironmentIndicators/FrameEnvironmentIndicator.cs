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
    /// <exception cref="ArgumentOutOfRangeException">
    ///     The value isn't one of the enumeration values.
    /// </exception>
    public EnvironmentIndicatorTabSize TabSize
    {
        get;
        set
        {
            if (!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            field = value;
        }
    } = EnvironmentIndicatorTabSize.Medium;

    internal FrameEnvironmentIndicator(string label)
        : base(label) { }
}
