namespace StellarAdmin.Dashboard;

/// <summary>
///     The layouts host pages can use to render inside the Dashboard shell.
/// </summary>
public static class StellarAdminLayouts
{
    /// <summary>
    ///     The Dashboard layout, with the sidebar, header and command palette.
    /// </summary>
    /// <remarks>
    ///     Set <c>ViewData["Title"]</c> to set the browser title.
    /// </remarks>
    public const string Dashboard = "/Areas/StellarAdmin/Views/Shared/_Layout.cshtml";
}
