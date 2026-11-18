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

    internal ResourcePartialLayoutOptions(string key, string partialName)
        : base(key) => PartialName = partialName;
}
