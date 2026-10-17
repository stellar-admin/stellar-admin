namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the default text of the edit page.
/// </summary>
public sealed class ResourceEditLabelOptions
{
    /// <summary>
    ///     Gets or sets the callback that generates the edit form's default submit button label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Save</c> followed by the singular resource label.
    /// </value>
    public Func<ResourceLabelContext, string> SubmitLabel { get; set; } =
        resource => $"Save {resource.SingularLabel}";

    /// <summary>
    ///     Gets or sets the callback that generates the edit page's default title.
    /// </summary>
    /// <value>
    ///     The default returns <c>Edit</c> followed by the singular resource label.
    /// </value>
    public Func<ResourceLabelContext, string> Title { get; set; } =
        resource => $"Edit {resource.SingularLabel}";
}
