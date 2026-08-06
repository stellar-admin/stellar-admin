namespace StellarAdmin.Shell;

/// <summary>
///     Provides a fluent API to configure the StellarAdmin shell.
/// </summary>
public class StellarAdminShellBuilder
{
    private readonly StellarAdminShellOptions _options;

    /// <summary>
    ///     Creates a new instance of <see cref="StellarAdminShellBuilder" />.
    /// </summary>
    /// <param name="options">The options the builder configures.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public StellarAdminShellBuilder(StellarAdminShellOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    ///     Links an additional stylesheet on every shell page.
    /// </summary>
    /// <param name="path">
    ///     The app-relative path of the stylesheet, e.g. <c>~/css/admin.css</c>. Registering the
    ///     same path more than once links it only once.
    /// </param>
    /// <exception cref="ArgumentException"></exception>
    public StellarAdminShellBuilder AddStylesheet(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!_options.Stylesheets.Contains(path))
        {
            _options.Stylesheets.Add(path);
        }

        return this;
    }
}
