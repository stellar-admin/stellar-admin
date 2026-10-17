namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the default text of the create page.
/// </summary>
public sealed class ResourceCreateLabelOptions
{
    /// <summary>
    ///     Gets or sets the callback that generates the create form's default submit button label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Create</c> followed by the singular resource label.
    /// </value>
    public Func<ResourceLabelContext, string> SubmitLabel { get; set; } =
        resource => $"Create {resource.SingularLabel}";

    /// <summary>
    ///     Gets or sets the callback that generates the create page's default title.
    /// </summary>
    /// <value>
    ///     The default returns <c>Create</c> followed by the singular resource label.
    /// </value>
    public Func<ResourceLabelContext, string> Title { get; set; } =
        resource => $"Create {resource.SingularLabel}";
}
