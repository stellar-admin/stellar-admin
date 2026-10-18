namespace StellarAdmin.UI;

/// <summary>
///     Specifies how an icon picker shows its icons.
/// </summary>
public enum IconPickerLayout
{
    /// <summary>
    ///     A button showing the chosen icon, which opens a popover where the user searches and
    ///     picks from a grid.
    /// </summary>
    Picker,

    /// <summary>
    ///     Every icon as a tile in a group, so the user picks with one click.
    /// </summary>
    /// <remarks>
    ///     Use it for a short list of icons.
    /// </remarks>
    Inline,
}

internal static class IconPickerLayoutExtensions
{
    extension(IconPickerLayout iconPickerLayout)
    {
        public string GetDataAttributeText() =>
            iconPickerLayout switch
            {
                IconPickerLayout.Picker => "picker",
                IconPickerLayout.Inline => "inline",
                _ => string.Empty,
            };
    }
}
