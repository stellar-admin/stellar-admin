using Microsoft.AspNetCore.Authorization;
using StellarAdmin.Infrastructure.Authorization;

namespace StellarAdmin.Widgets;

/// <summary>
///     Configures a widget.
/// </summary>
/// <typeparam name="TContext">The type of the context that describes the widget's page.</typeparam>
public sealed class WidgetBuilder<TContext>
    where TContext : WidgetContext
{
    private readonly WidgetOptions _options;

    /// <summary>
    ///     Sets a value that indicates whether the widget's handler runs while the page renders.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to load the widget with the page; otherwise,
    ///     <see langword="false" /> to load it after the page has rendered. The default is
    ///     <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     A widget without a handler always renders with the page. Handlers of widgets that
    ///     render with the page run one after another, since they share the request's services.
    /// </remarks>
    public bool Inline
    {
        set => _options.Inline = value;
    }

    /// <summary>
    ///     Sets where the widget is shown on a resource's page.
    /// </summary>
    /// <value>
    ///     The placement. The default is <see cref="WidgetPlacement.Top" />.
    /// </value>
    /// <remarks>
    ///     Widgets in a placement are shown in the order they're added. The home page has one
    ///     grid of widgets, so a placement other than <see cref="WidgetPlacement.Top" /> there is
    ///     a configuration problem, and the widget is shown in the grid.
    /// </remarks>
    public WidgetPlacement Placement
    {
        set => _options.Placement = value;
    }

    /// <summary>
    ///     Sets how often the widget loads again while the page is open.
    /// </summary>
    /// <value>
    ///     The time between loads, or <see langword="null" /> to load once. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     Only a widget with a handler loads again. When a load fails, the widget keeps what it
    ///     showed. An interval shorter than one second is a configuration problem, and the widget
    ///     loads once.
    /// </remarks>
    public TimeSpan? RefreshInterval
    {
        set => _options.RefreshInterval = value;
    }

    /// <summary>
    ///     Sets the number of the grid's 12 columns that the widget spans.
    /// </summary>
    /// <value>The number of columns, from 1 to 12. The default is 12.</value>
    /// <remarks>
    ///     The grid responds to the width it has, not the window's: in a narrow space a widget
    ///     spans half or all of the grid. A number outside 1 to 12 is a configuration problem,
    ///     and the widget spans 12 columns.
    /// </remarks>
    public int Span
    {
        set => _options.Span = value;
    }

    internal WidgetBuilder(WidgetOptions options) => _options = options;

    /// <summary>
    ///     Sets the delegate that returns the parameters passed to the widget's handler.
    /// </summary>
    /// <typeparam name="TParameters">The type of the parameters.</typeparam>
    /// <param name="parameters">
    ///     A delegate that returns the parameters from the page's context.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The delegate runs when the page renders. A widget that loads after the page has
    ///     rendered carries the parameters in its URL, serialized as JSON and protected with
    ///     ASP.NET Core Data Protection, so the handler can trust them. The handler must implement
    ///     <see cref="IWidgetHandler{TParameters, TModel}" /> for this type; otherwise, the
    ///     mismatch is a configuration problem, and the widget is left out.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="parameters" /> is <see langword="null" />.
    /// </exception>
    public WidgetBuilder<TContext> Parameters<TParameters>(Func<TContext, TParameters> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        _options.Parameters = context => parameters((TContext)context);
        _options.ParametersType = typeof(TParameters);

        return this;
    }

    /// <summary>
    ///     Requires an authenticated user for the widget.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A user who doesn't meet the widget's requirements doesn't see it.
    /// </remarks>
    public WidgetBuilder<TContext> RequireAuthorization()
    {
        return RequireAuthorization(Array.Empty<IAuthorizeData>());
    }

    /// <summary>
    ///     Requires the widget to satisfy an authorization policy.
    /// </summary>
    /// <param name="policy">The policy to satisfy.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A user who doesn't meet the widget's requirements doesn't see it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="policy" /> is <see langword="null" />.
    /// </exception>
    public WidgetBuilder<TContext> RequireAuthorization(AuthorizationPolicy policy)
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(policy));

        return this;
    }

    /// <summary>
    ///     Requires the widget to satisfy an authorization policy that a delegate builds.
    /// </summary>
    /// <param name="configurePolicy">A delegate that builds the policy.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A user who doesn't meet the widget's requirements doesn't see it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configurePolicy" /> is <see langword="null" />.
    /// </exception>
    public WidgetBuilder<TContext> RequireAuthorization(
        Action<AuthorizationPolicyBuilder> configurePolicy
    )
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(configurePolicy));

        return this;
    }

    /// <summary>
    ///     Requires the widget to satisfy the specified authorization data.
    /// </summary>
    /// <param name="authorizeData">
    ///     The authorization data to satisfy. Pass none to require only an authenticated user.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A user who doesn't meet the widget's requirements doesn't see it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="authorizeData" /> is <see langword="null" />.
    /// </exception>
    public WidgetBuilder<TContext> RequireAuthorization(params IAuthorizeData[] authorizeData)
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(authorizeData));

        return this;
    }

    /// <summary>
    ///     Requires the widget to satisfy the named authorization policies.
    /// </summary>
    /// <param name="policyNames">
    ///     The names of the policies to satisfy. Pass none to require only an authenticated user.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A user who doesn't meet the widget's requirements doesn't see it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="policyNames" /> is <see langword="null" />.
    /// </exception>
    public WidgetBuilder<TContext> RequireAuthorization(params string[] policyNames)
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(policyNames));

        return this;
    }

    /// <summary>
    ///     Sets the condition under which the widget is shown.
    /// </summary>
    /// <param name="predicate">
    ///     A delegate that returns <see langword="true" /> to show the widget for the page's
    ///     context.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The delegate runs when the page renders, before the widget loads.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="predicate" /> is <see langword="null" />.
    /// </exception>
    public WidgetBuilder<TContext> ShowWhen(Func<TContext, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        _options.ShowWhen = context => predicate((TContext)context);

        return this;
    }
}
