namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures a resource's create form.
/// </summary>
public abstract class ResourceCreateOptions : ResourceFormOptions
{
    /// <summary>
    ///     The model used by the create form.
    /// </summary>
    public abstract Type ModelType { get; }

    internal abstract object CreateModel();
}
