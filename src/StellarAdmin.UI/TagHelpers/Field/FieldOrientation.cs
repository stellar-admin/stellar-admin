namespace StellarAdmin.UI;

/// <summary>
///     How a field arranges its label, control, and supporting text.
/// </summary>
public enum FieldOrientation
{
    /// <summary>The label, control, and supporting text are stacked vertically.</summary>
    Vertical,

    /// <summary>The label, control, and supporting text are laid out horizontally.</summary>
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
