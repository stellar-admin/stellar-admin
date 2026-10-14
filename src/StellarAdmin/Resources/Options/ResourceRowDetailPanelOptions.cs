namespace StellarAdmin.Resources.Options;

/// <summary>
///     Configures content displayed in a row's details.
/// </summary>
public sealed class ResourceRowDetailPanelOptions
{
    /// <summary>
    ///     Whether the content loads when the row expands. Otherwise it renders with the index, with the row's resource
    ///     as its model.
    /// </summary>
    public bool LoadsOnDemand => Loader is not null;

    /// <summary>
    ///     The name that identifies the panel among a row's tabs, or null for the only panel.
    /// </summary>
    public string? Name { get; internal set; }

    /// <summary>
    ///     The name of the partial view that renders the content.
    /// </summary>
    public string PartialName { get; }

    /// <summary>
    ///     The label of the panel's tab, or null for the only panel.
    /// </summary>
    public string? Title { get; }

    internal Func<IServiceProvider, string, CancellationToken, Task<object?>>? Loader { get; }

    internal ResourceRowDetailPanelOptions(
        string partialName,
        Func<IServiceProvider, string, CancellationToken, Task<object?>>? loader,
        string? title = null
    )
    {
        PartialName = partialName;
        Loader = loader;
        Title = title;
    }
}
