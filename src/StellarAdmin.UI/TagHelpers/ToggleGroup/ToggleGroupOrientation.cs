namespace StellarAdmin.UI;

/// <summary>
///     Specifies the direction in which a toggle group arranges its items.
/// </summary>
public enum ToggleGroupOrientation
{
    /// <summary>
    ///     The items are arranged in a row.
    /// </summary>
    Horizontal,

    /// <summary>
    ///     The items are arranged in a column.
    /// </summary>
    Vertical,
}

internal static class ToggleGroupOrientationExtensions
{
    extension(ToggleGroupOrientation orientation)
    {
        public string GetDataAttributeText() =>
            orientation switch
            {
                ToggleGroupOrientation.Horizontal => "horizontal",
                ToggleGroupOrientation.Vertical => "vertical",
                _ => string.Empty,
            };
    }
}
