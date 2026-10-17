namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a toggle.
/// </summary>
public enum ToggleSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     A small toggle.
    /// </summary>
    Small,

    /// <summary>
    ///     A large toggle.
    /// </summary>
    Large,
}

internal static class ToggleSizeExtensions
{
    extension(ToggleSize size)
    {
        public string GetDataAttributeText() =>
            size switch
            {
                ToggleSize.Default => "default",
                ToggleSize.Small => "sm",
                ToggleSize.Large => "lg",
                _ => string.Empty,
            };
    }
}
