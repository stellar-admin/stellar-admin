namespace StellarAdmin;

/// <summary>
///     Provides the layouts that application pages can use to render inside the admin panel.
/// </summary>
public static class StellarAdminLayouts
{
    /// <summary>
    ///     Represents the default admin panel layout, with the sidebar, header and command palette.
    /// </summary>
    /// <remarks>
    ///     Set <c>ViewData["Title"]</c> to set the browser title.
    /// </remarks>
    public const string Default = "/Areas/StellarAdmin/Views/Shared/_Layout.cshtml";
}
