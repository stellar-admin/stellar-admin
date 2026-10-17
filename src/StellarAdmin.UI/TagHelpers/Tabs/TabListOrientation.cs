namespace StellarAdmin.UI;

/// <summary>
///     Specifies the direction in which a tab list arranges its tabs.
/// </summary>
public enum TabListOrientation
{
    /// <summary>
    ///     The tabs are arranged in a row.
    /// </summary>
    Horizontal,

    /// <summary>
    ///     The tabs are arranged in a column.
    /// </summary>
    Vertical,
}

internal static class TabListOrientationExtensions
{
    extension(TabListOrientation orientation)
    {
        public string GetDataAttributeText() =>
            orientation switch
            {
                TabListOrientation.Horizontal => "horizontal",
                TabListOrientation.Vertical => "vertical",
                _ => string.Empty,
            };
    }
}
