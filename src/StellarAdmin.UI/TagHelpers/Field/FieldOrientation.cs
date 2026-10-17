namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a field arranges its label, control and supporting text.
/// </summary>
public enum FieldOrientation
{
    /// <summary>
    ///     Stacks the label, control and supporting text.
    /// </summary>
    Vertical,

    /// <summary>
    ///     Places the label, control and supporting text side by side.
    /// </summary>
    Horizontal,
}

internal static class FieldOrientationExtensions
{
    extension(FieldOrientation orientation)
    {
        public string GetDataAttributeText() =>
            orientation switch
            {
                FieldOrientation.Vertical => "vertical",
                FieldOrientation.Horizontal => "horizontal",
                _ => string.Empty,
            };
    }
}
