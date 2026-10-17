namespace StellarAdmin.Sidebar;

/// <summary>
///     Represents a user menu item that links to an MVC controller action.
/// </summary>
public sealed class ActionUserMenuItem : UserMenuItem
{
    /// <summary>
    ///     Gets the name of the action.
    /// </summary>
    public string ActionName { get; }

    /// <summary>
    ///     Gets the name of the area, or <see langword="null" /> for an action outside any area.
    /// </summary>
    public string? Area { get; }

    /// <summary>
    ///     Gets the name of the controller.
    /// </summary>
    public string ControllerName { get; }

    /// <summary>
    ///     Gets the name of the icon shown before the text, or <see langword="null" /> for none.
    /// </summary>
    public string? Icon { get; internal set; }

    /// <summary>
    ///     Gets the text of the item.
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
