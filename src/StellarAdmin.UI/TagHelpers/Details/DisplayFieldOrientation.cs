namespace StellarAdmin.UI;

/// <summary>
///     How a display field arranges its label and value.
/// </summary>
public enum DisplayFieldOrientation
{
    /// <summary>The label sits above the value.</summary>
    Vertical,

    /// <summary>The label sits beside the value.</summary>
    Horizontal,
}

internal static class DisplayFieldOrientationExtensions
{
    extension(DisplayFieldOrientation orientation)
    {
        public string GetDataAttributeText() =>
            orientation switch
            {
                DisplayFieldOrientation.Vertical => "vertical",
                DisplayFieldOrientation.Horizontal => "horizontal",
                _ => string.Empty,
            };
    }
}
