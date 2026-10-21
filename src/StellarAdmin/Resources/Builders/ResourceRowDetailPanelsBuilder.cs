using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Adds the panels shown in a row's details.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
/// <remarks>
///     Two or more panels are shown as tabs.
/// </remarks>
public abstract class ResourceRowDetailPanelsBuilder<TResource>
{
    private readonly List<ResourceRowDetailPanelOptions> _panels = [];

    /// <summary>
    ///     Gets the service collection that the panels' handlers are registered in.
    /// </summary>
    protected IServiceCollection Services { get; }

    internal IReadOnlyList<ResourceRowDetailPanelOptions> Panels => _panels;

    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="ResourceRowDetailPanelsBuilder{TResource}" /> class.
    /// </summary>
    /// <param name="services">The service collection to register the panels' handlers in.</param>
    protected ResourceRowDetailPanelsBuilder(IServiceCollection services) => Services = services;

    /// <summary>
    ///     Adds a panel that shows a partial view.
    /// </summary>
    /// <param name="title">The title of the panel's tab.</param>
    /// <param name="partialName">The partial view to render.</param>
    /// <returns>A builder that configures the panel.</returns>
    /// <remarks>
    ///     The partial view is rendered with the index page, with the row's resource as its model.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="title" /> or <paramref name="partialName" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="title" /> or <paramref name="partialName" /> is empty or whitespace.
    /// </exception>
    public ResourceRowDetailPanelBuilder Add(string title, string partialName) =>
        AddPanel(title, partialName, null);

    /// <summary>
    ///     Adds a panel that shows a partial view with a model that a handler loads.
    /// </summary>
    /// <typeparam name="TModel">The type of the partial view's model.</typeparam>
    /// <typeparam name="THandler">The type of the handler that loads the model.</typeparam>
    /// <param name="title">The title of the panel's tab.</param>
    /// <param name="partialName">The partial view to render.</param>
    /// <returns>A builder that configures the panel.</returns>
    /// <remarks>
    ///     The handler loads the model when the panel is first shown. The handler is registered as
    ///     a scoped service if it isn't registered already.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="title" /> or <paramref name="partialName" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="title" /> or <paramref name="partialName" /> is empty or whitespace.
    /// </exception>
    public ResourceRowDetailPanelBuilder Add<TModel, THandler>(string title, string partialName)
        where THandler : class, IResourceRowDetailHandler<TModel>
    {
        Services.TryAddScoped<THandler>();

        return AddPanel(title, partialName, new HandlerRowDetailLoader<TModel, THandler>());
    }

    // "Order history" becomes "order-history"
    private static string CreateName(string title)
    {
        var name = new StringBuilder(title.Length);
        foreach (var character in title.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                name.Append(character);
            }
            else if (name.Length > 0 && name[^1] != '-')
            {
                name.Append('-');
            }
        }

        return name.ToString().TrimEnd('-');
    }

    /// <summary>
    ///     Adds a panel that shows a partial view.
    /// </summary>
    /// <param name="title">The title of the panel's tab.</param>
    /// <param name="partialName">The partial view to render.</param>
    /// <param name="loader">
    ///     The object that loads the partial's model from the row's key when the panel is first
    ///     shown, or <see langword="null" /> to use the row itself as the model.
    /// </param>
    /// <returns>A builder that configures the panel.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="title" /> or <paramref name="partialName" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     An empty or white space title or partial name is a configuration problem, and the panel
    ///     is left out.
    /// </remarks>
    protected ResourceRowDetailPanelBuilder AddPanel(
        string title,
        string partialName,
        ResourceRowDetailLoader? loader
    )
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(partialName);

        var panel = new ResourceRowDetailPanelOptions(partialName, loader, title)
        {
            Name = CreateName(title),
        };
        _panels.Add(panel);

        return new(panel);
    }
}
