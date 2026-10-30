namespace StellarAdmin.Widgets;

/// <summary>
///     Specifies where a widget is shown on a resource's page.
/// </summary>
public enum WidgetPlacement
{
    /// <summary>
    ///     Above the page's content.
    /// </summary>
    Top,

    /// <summary>
    ///     In a column beside the page's content, or below it when the page is narrow.
    /// </summary>
    Aside,

    /// <summary>
    ///     Below the page's content.
    /// </summary>
    Bottom,
}
