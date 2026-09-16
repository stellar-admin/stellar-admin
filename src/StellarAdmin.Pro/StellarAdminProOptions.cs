namespace StellarAdmin.Pro;

/// <summary>
///     Options for StellarAdmin Pro.
/// </summary>
public class StellarAdminProOptions
{
    /// <summary>
    ///     Gets the app-relative paths of the scripts the shell layout links, in registration order.
    /// </summary>
    public IList<string> Scripts { get; } = [];

    /// <summary>
    ///     Gets the app-relative paths of the stylesheets the shell layout links, in registration order.
    /// </summary>
    public IList<string> Stylesheets { get; } = [];
}
