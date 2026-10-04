namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Default page text for resources.
/// </summary>
public sealed class ResourceLabelOptions
{
    /// <summary>
    ///     Default create page text.
    /// </summary>
    public ResourceCreateLabelOptions Create { get; } = new();

    /// <summary>
    ///     Default delete confirmation text.
    /// </summary>
    public ResourceDeleteLabelOptions Delete { get; } = new();

    /// <summary>
    ///     Default edit page text.
    /// </summary>
    public ResourceEditLabelOptions Edit { get; } = new();

    /// <summary>
    ///     Default index page text.
    /// </summary>
    public ResourceIndexLabelOptions Index { get; } = new();

    /// <summary>
    ///     Default lookup editor text.
    /// </summary>
    public LookupLabelOptions Lookup { get; } = new();
}
