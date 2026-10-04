namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Default index page text.
/// </summary>
public sealed class ResourceIndexLabelOptions
{
    /// <summary>
    ///     The callback that generates the default index create button label.
    /// </summary>
    public Func<ResourceLabelContext, string> CreateLabel { get; set; } = resource => "Create";

    /// <summary>
    ///     The callback that generates the default index delete button label.
    /// </summary>
    public Func<ResourceLabelContext, string> DeleteLabel { get; set; } =
        resource => $"Delete {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default index edit link label.
    /// </summary>
    public Func<ResourceLabelContext, string> EditLabel { get; set; } =
        resource => $"Edit {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default index search placeholder.
    /// </summary>
    public Func<ResourceLabelContext, string> SearchPlaceholder { get; set; } =
        resource => $"Search {resource.PluralLabel}...";

    /// <summary>
    ///     The callback that generates the default index page title.
    /// </summary>
    public Func<ResourceLabelContext, string> Title { get; set; } =
        resource => resource.PluralLabel;
}
