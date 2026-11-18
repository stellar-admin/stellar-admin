namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of one layout of a resource's index page.
/// </summary>
public abstract class ResourceLayoutOptions
{
    /// <summary>
    ///     Gets the key that identifies the layout.
    /// </summary>
    public string Key { get; }

    private protected ResourceLayoutOptions(string key) => Key = key;
}
