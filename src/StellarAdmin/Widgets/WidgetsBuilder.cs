using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;

namespace StellarAdmin.Widgets;

/// <summary>
///     Adds the widgets shown on a page.
/// </summary>
/// <typeparam name="TContext">The type of the context that describes the page.</typeparam>
/// <remarks>
///     Widgets are shown in the order they're added, in a grid of 12 columns.
/// </remarks>
public sealed class WidgetsBuilder<TContext>
    where TContext : WidgetContext
{
    private readonly IServiceCollection _services;
    private readonly List<WidgetOptions> _widgets;

    internal WidgetsBuilder(IServiceCollection services, List<WidgetOptions> widgets)
    {
        _services = services;
        _widgets = widgets;
    }

    /// <summary>
    ///     Adds a widget that shows a partial view.
    /// </summary>
    /// <param name="name">The name that identifies the widget on the page.</param>
    /// <param name="partialName">The partial view to render.</param>
    /// <returns>A builder that configures the widget.</returns>
    /// <remarks>
    ///     The partial view renders with the page, with the page's context as its model. The name
    ///     isn't shown; the partial view renders the widget's heading, if any. An empty or white
    ///     space name or partial name, or a name another widget on the page has, is a
    ///     configuration problem, and the widget is left out.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="name" /> or <paramref name="partialName" /> is <see langword="null" />.
    /// </exception>
    public WidgetBuilder<TContext> Add(string name, string partialName) =>
        AddWidget(name, partialName, null);

    /// <summary>
    ///     Adds a widget that shows a partial view, and configures the widget.
    /// </summary>
    /// <param name="name">The name that identifies the widget on the page.</param>
    /// <param name="partialName">The partial view to render.</param>
    /// <param name="configure">A delegate that configures the widget.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The partial view renders with the page, with the page's context as its model. The name
    ///     isn't shown; the partial view renders the widget's heading, if any. An empty or white
    ///     space name or partial name, or a name another widget on the page has, is a
    ///     configuration problem, and the widget is left out.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="name" />, <paramref name="partialName" /> or
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public WidgetsBuilder<TContext> Add(
        string name,
        string partialName,
        Action<WidgetBuilder<TContext>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(Add(name, partialName));

        return this;
    }

    /// <summary>
    ///     Adds a widget that shows a partial view with a model that a handler loads.
    /// </summary>
    /// <typeparam name="THandler">
    ///     The type of the handler that loads the model, which implements
    ///     <see cref="IWidgetHandler{TModel}" /> or
    ///     <see cref="IWidgetHandler{TParameters, TModel}" /> for the partial view's model.
    /// </typeparam>
    /// <param name="name">The name that identifies the widget on the page.</param>
    /// <param name="partialName">The partial view to render.</param>
    /// <returns>A builder that configures the widget.</returns>
    /// <remarks>
    ///     The widget loads after the page has rendered, unless <c>Inline</c> is set. The handler
    ///     is registered as a scoped service if it isn't registered already. A handler that
    ///     doesn't implement exactly one widget handler interface is a configuration problem, and
    ///     the widget is left out.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="name" /> or <paramref name="partialName" /> is <see langword="null" />.
    /// </exception>
    public WidgetBuilder<TContext> Add<THandler>(string name, string partialName)
        where THandler : class =>
        AddWidget(name, partialName, WidgetLoader.Create(typeof(THandler), _services));

    /// <summary>
    ///     Adds a widget that shows a partial view with a model that a handler loads, and
    ///     configures the widget.
    /// </summary>
    /// <typeparam name="THandler">
    ///     The type of the handler that loads the model, which implements
    ///     <see cref="IWidgetHandler{TModel}" /> or
    ///     <see cref="IWidgetHandler{TParameters, TModel}" /> for the partial view's model.
    /// </typeparam>
    /// <param name="name">The name that identifies the widget on the page.</param>
    /// <param name="partialName">The partial view to render.</param>
    /// <param name="configure">A delegate that configures the widget.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The widget loads after the page has rendered, unless <c>Inline</c> is set. The handler
    ///     is registered as a scoped service if it isn't registered already. A handler that
    ///     doesn't implement exactly one widget handler interface is a configuration problem, and
    ///     the widget is left out.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="name" />, <paramref name="partialName" /> or
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public WidgetsBuilder<TContext> Add<THandler>(
        string name,
        string partialName,
        Action<WidgetBuilder<TContext>> configure
    )
        where THandler : class
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(Add<THandler>(name, partialName));

        return this;
    }

    private WidgetBuilder<TContext> AddWidget(string name, string partialName, WidgetLoader? loader)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(partialName);

        var widget = new WidgetOptions(name, partialName, loader)
        {
            Source = ConfigurationSource.Capture(),
        };
        _widgets.Add(widget);

        return new(widget);
    }
}
