namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a switch.
/// </summary>
public enum SwitchSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     A smaller, more compact switch.
    /// </summary>
    Small,
}

internal static class SwitchSizeExtensions
{
    extension(SwitchSize size)
    {
        public string GetDataAttributeText() =>
            size switch
            {
                SwitchSize.Default => "default",
                SwitchSize.Small => "sm",
                _ => string.Empty,
            };
    }
}
