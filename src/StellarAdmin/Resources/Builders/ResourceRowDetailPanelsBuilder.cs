using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the panels of a row's details, shown as tabs.
/// </summary>
public class ResourceRowDetailPanelsBuilder<TResource>
{
    private readonly List<ResourceRowDetailPanelOptions> _panels = [];

    /// <summary>
    ///     The services the panels' handlers are registered in.
    /// </summary>
    protected IServiceCollection Services { get; }

    internal IReadOnlyList<ResourceRowDetailPanelOptions> Panels => _panels;

    /// <summary>
    ///     Creates a panels builder.
    /// </summary>
    protected internal ResourceRowDetailPanelsBuilder(IServiceCollection services) =>
        Services = services;

    /// <summary>
    ///     Adds a panel that renders a partial view with the index, with the row's resource as its model.
    /// </summary>
    public ResourceRowDetailPanelBuilder Add(string title, string partialName) =>
        AddPanel(title, partialName, null);

    /// <summary>
    ///     Adds a panel that renders a partial view with a model the handler loads when the panel is first shown.
    /// </summary>
    public ResourceRowDetailPanelBuilder Add<TModel, THandler>(string title, string partialName)
        where THandler : class, IResourceRowDetailHandler<TModel>
    {
        Services.TryAddScoped<THandler>();

        return AddPanel(title, partialName, CreateHandlerLoader<TModel, THandler>());
    }

    internal static Func<
        IServiceProvider,
        string,
        CancellationToken,
        Task<object?>
    > CreateHandlerLoader<TModel, THandler>()
        where THandler : class, IResourceRowDetailHandler<TModel> =>
        async (services, id, cancellationToken) =>
            await services.GetRequiredService<THandler>().FindAsync(id, cancellationToken);

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
    ///     Adds a panel with a model the loader returns for a resource key when the panel is first shown, or with the
    ///     row's resource when the loader is null.
    /// </summary>
    protected ResourceRowDetailPanelBuilder AddPanel(
        string title,
        string partialName,
        Func<IServiceProvider, string, CancellationToken, Task<object?>>? loader
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(partialName);

        var panel = new ResourceRowDetailPanelOptions(partialName, loader, title)
        {
            Name = CreateName(title),
        };
        _panels.Add(panel);

        return new(panel);
    }
}
