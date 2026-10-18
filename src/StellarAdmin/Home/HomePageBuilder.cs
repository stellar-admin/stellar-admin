namespace StellarAdmin.Home;

/// <summary>
///     Configures the admin panel's home page.
/// </summary>
public sealed class HomePageBuilder
{
    private readonly HomePageOptions _options;

    /// <summary>
    ///     Sets a value that indicates whether the home page shows the setup checklist.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to show the setup checklist; otherwise,
    ///     <see langword="false" />. The default is <see langword="true" />.
    /// </value>
    /// <remarks>
    ///     The checklist is only shown in the Development environment.
    /// </remarks>
    public bool ShowSetupChecklist
    {
        set => _options.ShowSetupChecklist = value;
    }

    internal HomePageBuilder(HomePageOptions options) => _options = options;
}
