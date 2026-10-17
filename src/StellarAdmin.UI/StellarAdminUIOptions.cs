namespace StellarAdmin.UI;

/// <summary>
///     Provides the options for the StellarAdmin tag helpers.
/// </summary>
/// <seealso cref="StellarAdminUIBuilder" />
public sealed class StellarAdminUIOptions
{
    /// <summary>
    ///     Gets the look of dropdown menus.
    /// </summary>
    public StellarAdminUIMenuOptions Menu { get; } = new();
}
