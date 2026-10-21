namespace StellarAdmin.Diagnostics;

/// <summary>
///     Configures how the admin panel reports problems in its configuration.
/// </summary>
/// <remarks>
///     The admin panel works around a configuration problem instead of stopping the app, and
///     logs it once in the <c>StellarAdmin.Configuration</c> category.
/// </remarks>
public sealed class DiagnosticsBuilder
{
    private readonly DiagnosticsOptions _options;

    /// <summary>
    ///     Sets a value that indicates whether admin panel pages show an indicator that lists the
    ///     configuration problems.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to show the indicator; otherwise, <see langword="false" />.
    ///     The default is <see langword="true" />.
    /// </value>
    /// <remarks>
    ///     The indicator is only shown in the Development environment, and only when there are
    ///     problems. The problems are logged either way.
    /// </remarks>
    public bool ShowIndicator
    {
        set => _options.ShowIndicator = value;
    }

    /// <summary>
    ///     Sets a value that indicates whether a configuration problem stops the app from
    ///     starting.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to throw an <see cref="InvalidOperationException" /> that lists
    ///     the problems when the app starts; otherwise, <see langword="false" />. The default is
    ///     <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     This applies in every environment. Set it from the environment to fail only tests or
    ///     builds.
    /// </remarks>
    public bool ThrowOnProblems
    {
        set => _options.ThrowOnProblems = value;
    }

    internal DiagnosticsBuilder(DiagnosticsOptions options) => _options = options;
}
