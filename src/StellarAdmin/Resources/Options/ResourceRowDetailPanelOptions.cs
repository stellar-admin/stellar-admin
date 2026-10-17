namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of content displayed in a row's details.
/// </summary>
public sealed class ResourceRowDetailPanelOptions
{
    /// <summary>
    ///     Gets a value that indicates whether the content loads when the row expands.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the content loads when the row expands; otherwise,
    ///     <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Content that doesn't load on demand renders with the index, with the row's resource as
    ///     its model.
    /// </remarks>
    public bool LoadsOnDemand => Loader is not null;

    /// <summary>
    ///     Gets the name that identifies the panel among a row's tabs.
    /// </summary>
    /// <value>
    ///     The name, or <see langword="null" /> when the panel is the only one.
    /// </value>
    public string? Name { get; internal set; }

    /// <summary>
    ///     Gets the name of the partial view that renders the content.
    /// </summary>
    public string PartialName { get; }

    /// <summary>
    ///     Gets the label of the panel's tab.
    /// </summary>
    /// <value>
    ///     The label, or <see langword="null" /> when the panel is the only one.
    /// </value>
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
