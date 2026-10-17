namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a <c>&lt;sa-select&gt;</c>.
/// </summary>
public enum SelectSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     A smaller, more compact size.
    /// </summary>
    Small,
}

internal static class SelectSizeExtensions
{
    extension(SelectSize size)
    {
        public string GetDataAttributeText() =>
            size switch
            {
                SelectSize.Default => "default",
                SelectSize.Small => "sm",
                _ => "",
            };
    }
}
