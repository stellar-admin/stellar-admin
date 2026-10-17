namespace StellarAdmin.UI;

/// <summary>
///     Specifies the visual style of a tab list.
/// </summary>
public enum TabListVariant
{
    /// <summary>
    ///     Tabs on a muted background.
    /// </summary>
    Default,

    /// <summary>
    ///     Tabs with an underline below the selected tab.
    /// </summary>
    Line,
}

internal static class TabListVariantExtensions
{
    extension(TabListVariant variant)
    {
        public string GetDataAttributeText() =>
            variant switch
            {
                TabListVariant.Default => "default",
                TabListVariant.Line => "line",
                _ => "",
            };
    }
}
