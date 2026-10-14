namespace StellarAdmin.Resources.Options;

/// <summary>
///     Configures a resource's edit form.
/// </summary>
public abstract class ResourceEditOptions : ResourceFormOptions
{
    /// <summary>
    ///     The model used by the edit form.
    /// </summary>
    public abstract Type ModelType { get; }
}
