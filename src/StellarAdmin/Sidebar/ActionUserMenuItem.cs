namespace StellarAdmin.Sidebar;

/// <summary>
///     A user menu item that links to an MVC controller action.
/// </summary>
public sealed class ActionUserMenuItem : UserMenuItem
{
    /// <summary>
    ///     The action name.
    /// </summary>
    public string ActionName { get; }

    /// <summary>
    ///     The area name, or <c>null</c> for an action outside any area.
    /// </summary>
    public string? Area { get; }

    /// <summary>
    ///     The controller name.
    /// </summary>
    public string ControllerName { get; }

    /// <summary>
    ///     The icon shown before the text, or <c>null</c> for none.
    /// </summary>
    public string? Icon { get; internal set; }

    /// <summary>
    ///     The item text.
    /// </summary>
    public string Text { get; }

    internal ActionUserMenuItem(string text, string action, string controller, string? area)
    {
        Text = text;
        ActionName = action;
        ControllerName = controller;
        Area = area;
    }
}
