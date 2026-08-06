namespace StellarAdmin.Shell;

/// <summary>
///     Options for the StellarAdmin shell.
/// </summary>
public class StellarAdminShellOptions
{
    /// <summary>
    ///     Gets the app-relative paths of the stylesheets the shell layout links, in registration order.
    /// </summary>
    public IList<string> Stylesheets { get; } = [];
}
