namespace StellarAdmin.Resources.Options;

/// <summary>
///     Default index page text.
/// </summary>
public sealed class ResourceIndexLabelOptions
{
    /// <summary>
    ///     The callback that generates the default inline row cancel button label.
    /// </summary>
    public Func<ResourceLabelContext, string> CancelLabel { get; set; } = resource => "Cancel";

    /// <summary>
    ///     The callback that generates the default index create button label.
    /// </summary>
    public Func<ResourceLabelContext, string> CreateLabel { get; set; } = resource => "Create";

    /// <summary>
    ///     The callback that generates the default message of the toast shown when a resource is
    ///     added in a new row.
    /// </summary>
    public Func<ResourceLabelContext, string> CreatedMessage { get; set; } =
        resource => $"{resource.SingularLabel} added";

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
    ///     The callback that generates the default description of the toast shown when a saved
    ///     inline row isn't on the reloaded page.
    /// </summary>
    public Func<ResourceLabelContext, string> NotShownMessage { get; set; } =
        resource => "It isn't on this page with the current search, scope and sort.";

    /// <summary>
    ///     The callback that generates the default inline row save button label.
    /// </summary>
    public Func<ResourceLabelContext, string> SaveLabel { get; set; } = resource => "Save";

    /// <summary>
    ///     The callback that generates the default message of the toast shown when an edited inline
    ///     row isn't on the reloaded page.
    /// </summary>
    public Func<ResourceLabelContext, string> SavedMessage { get; set; } =
        resource => $"{resource.SingularLabel} saved";

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

    /// <summary>
    ///     The callback that generates the default index view link label.
    /// </summary>
    public Func<ResourceLabelContext, string> ViewLabel { get; set; } =
        resource => $"View {resource.SingularLabel}";
}
