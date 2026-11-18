namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of an index layout that renders the resources with a partial view.
/// </summary>
public sealed class ResourcePartialLayoutOptions : ResourceLayoutOptions
{
    /// <summary>
    ///     Gets the name of the partial view that renders the layout.
    /// </summary>
    public string PartialName { get; }

    // Lists the items of a layout with a handler, or null for one that shows the resource's own rows
    internal ResourceLayoutLoader? Loader { get; }

    internal ResourcePartialLayoutOptions(
        string key,
        string partialName,
        ResourceLayoutLoader? loader = null
    )
        : base(key)
    {
        PartialName = partialName;
        Loader = loader;
    }
}
