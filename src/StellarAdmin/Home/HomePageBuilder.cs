using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Widgets;

namespace StellarAdmin.Home;

/// <summary>
///     Configures the admin panel's home page.
/// </summary>
public sealed class HomePageBuilder
{
    private readonly HomePageOptions _options;
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets a value that indicates whether the home page shows the setup checklist.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to show the setup checklist; otherwise,
    ///     <see langword="false" />. The default is <see langword="true" />.
    /// </value>
    /// <remarks>
    ///     The checklist is only shown in the Development environment, above the home page's
    ///     widgets.
    /// </remarks>
    public bool ShowSetupChecklist
    {
        set => _options.ShowSetupChecklist = value;
    }

    internal HomePageBuilder(HomePageOptions options, IServiceCollection services)
    {
        _options = options;
        _services = services;
    }

    /// <summary>
    ///     Configures the widgets shown on the home page.
    /// </summary>
    /// <param name="configure">A delegate that adds the widgets.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     When the current user can see at least one widget, the home page shows the widgets in
    ///     place of its links to the resources.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public HomePageBuilder Widgets(Action<WidgetsBuilder<WidgetContext>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services, _options.Widgets));

        return this;
    }
}
