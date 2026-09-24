namespace StellarAdmin.Dashboard.Areas.StellarAdmin;

/// <summary>
///     The <c>TempData</c> keys the StellarAdmin views communicate through.
/// </summary>
public static class TempDataKeys
{
    /// <summary>
    ///     Carries rejected edit-page deletion messages back to the edit form.
    /// </summary>
    public const string DeleteErrors = "StellarAdminDeleteErrors";

    /// <summary>
    ///     Carries an error message into the next page render, shown as a destructive
    ///     alert above the index page's grid.
    /// </summary>
    public const string ErrorMessage = "StellarAdminErrorMessage";
}
