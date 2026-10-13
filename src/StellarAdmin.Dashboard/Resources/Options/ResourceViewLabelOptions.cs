namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Default view page text.
/// </summary>
public sealed class ResourceViewLabelOptions
{
    /// <summary>
    ///     The callback that generates the default view page title.
    /// </summary>
    public Func<ResourceLabelContext, string> Title { get; set; } =
        resource => $"View {resource.SingularLabel}";
}
