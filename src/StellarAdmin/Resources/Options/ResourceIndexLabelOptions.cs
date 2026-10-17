namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the default text of the index page.
/// </summary>
public sealed class ResourceIndexLabelOptions
{
    /// <summary>
    ///     Gets or sets the delegate that returns the default label of the cancel button on an
    ///     inline row.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "Cancel".
    /// </value>
    public Func<ResourceLabelContext, string> CancelLabel { get; set; } = resource => "Cancel";

    /// <summary>
    ///     Gets or sets the delegate that returns the default label of the create button on the
    ///     index page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "Create".
    /// </value>
    public Func<ResourceLabelContext, string> CreateLabel { get; set; } = resource => "Create";

    /// <summary>
    ///     Gets or sets the delegate that returns the default message of the toast shown when a
    ///     resource is added in a new row.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns the resource's
    ///     singular label followed by "added".
    /// </value>
    public Func<ResourceLabelContext, string> CreatedMessage { get; set; } =
        resource => $"{resource.SingularLabel} added";

    /// <summary>
    ///     Gets or sets the delegate that returns the default label of the delete button on the
    ///     index page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "Delete" followed
    ///     by the resource's singular label.
    /// </value>
    public Func<ResourceLabelContext, string> DeleteLabel { get; set; } =
        resource => $"Delete {resource.SingularLabel}";

    /// <summary>
    ///     Gets or sets the delegate that returns the default label of the edit link on the index
    ///     page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "Edit" followed by
    ///     the resource's singular label.
    /// </value>
    public Func<ResourceLabelContext, string> EditLabel { get; set; } =
        resource => $"Edit {resource.SingularLabel}";

    /// <summary>
    ///     Gets or sets the delegate that returns the default description of the toast shown when a
    ///     saved inline row isn't on the reloaded page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "It isn't on this
    ///     page with the current search, scope and sort.".
    /// </value>
    public Func<ResourceLabelContext, string> NotShownMessage { get; set; } =
        resource => "It isn't on this page with the current search, scope and sort.";

    /// <summary>
    ///     Gets or sets the delegate that returns the default label of the save button on an inline
    ///     row.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "Save".
    /// </value>
    public Func<ResourceLabelContext, string> SaveLabel { get; set; } = resource => "Save";

    /// <summary>
    ///     Gets or sets the delegate that returns the default message of the toast shown when an
    ///     edited inline row isn't on the reloaded page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns the resource's
    ///     singular label followed by "saved".
    /// </value>
    public Func<ResourceLabelContext, string> SavedMessage { get; set; } =
        resource => $"{resource.SingularLabel} saved";

    /// <summary>
    ///     Gets or sets the delegate that returns the default placeholder of the search box on the
    ///     index page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "Search" followed
    ///     by the resource's plural label and "...".
    /// </value>
    public Func<ResourceLabelContext, string> SearchPlaceholder { get; set; } =
        resource => $"Search {resource.PluralLabel}...";

    /// <summary>
    ///     Gets or sets the delegate that returns the default title of the index page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns the resource's
    ///     plural label.
    /// </value>
    public Func<ResourceLabelContext, string> Title { get; set; } =
        resource => resource.PluralLabel;

    /// <summary>
    ///     Gets or sets the delegate that returns the default label of the view link on the index
    ///     page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "View" followed by
    ///     the resource's singular label.
    /// </value>
    public Func<ResourceLabelContext, string> ViewLabel { get; set; } =
        resource => $"View {resource.SingularLabel}";
}
