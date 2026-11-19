namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of an index data layout that renders the resources with a partial
///     view.
/// </summary>
public sealed class ResourcePartialDataLayoutOptions : ResourceDataLayoutOptions
{
    /// <summary>
    ///     Gets the name of the partial view that renders the data layout.
    /// </summary>
    public string PartialName { get; }

    // Lists the items of a data layout with a handler, or null for one that shows the resource's own rows
    internal ResourceDataLayoutLoader? Loader { get; }

    internal ResourcePartialDataLayoutOptions(
        string key,
        string partialName,
        ResourceDataLayoutLoader? loader = null
    )
        : base(key)
    {
        PartialName = partialName;
        Loader = loader;
    }
}
