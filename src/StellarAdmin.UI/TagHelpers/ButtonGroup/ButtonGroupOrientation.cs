namespace StellarAdmin.UI;

/// <summary>
///     Specifies the direction in which a button group arranges its items.
/// </summary>
public enum ButtonGroupOrientation
{
    /// <summary>
    ///     Items are arranged in a row.
    /// </summary>
    Horizontal,

    /// <summary>
    ///     Items are stacked in a column.
    /// </summary>
    Vertical,
}

internal static class ButtonGroupOrientationExtensions
{
    extension(ButtonGroupOrientation orientation)
    {
        public string GetDataAttributeText()
        {
            return orientation switch
            {
                ButtonGroupOrientation.Horizontal => "horizontal",
                ButtonGroupOrientation.Vertical => "vertical",
                _ => string.Empty,
            };
        }
    }
}
