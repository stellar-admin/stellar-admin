namespace StellarAdmin;

/// <summary>
///     The layouts host pages can use to render inside the admin panel shell.
/// </summary>
public static class StellarAdminLayouts
{
    /// <summary>
    ///     The default admin panel layout, with the sidebar, header and command palette.
    /// </summary>
    /// <remarks>
    ///     Set <c>ViewData["Title"]</c> to set the browser title.
    /// </remarks>
    public const string Default = "/Areas/StellarAdmin/Views/Shared/_Layout.cshtml";
}
