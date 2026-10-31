namespace StellarAdmin.UI;

/// <summary>
///     Specifies what a horizontal tab list does with the tabs that don't fit on one line.
/// </summary>
public enum TabListOverflow
{
    /// <summary>
    ///     The tabs that don't fit move into a menu at the end of the list. The selected tab
    ///     always stays in the list.
    /// </summary>
    Menu,

    /// <summary>
    ///     All tabs stay in the list.
    /// </summary>
    None,
}
