namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the default text of the delete confirmation.
/// </summary>
public sealed class ResourceDeleteLabelOptions
{
    /// <summary>
    ///     Gets or sets the callback that generates the delete confirmation's default cancel button
    ///     label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Cancel</c>.
    /// </value>
    public Func<ResourceLabelContext, string> CancelLabel { get; set; } = resource => "Cancel";

    /// <summary>
    ///     Gets or sets the callback that generates the delete confirmation's default confirm
    ///     button label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Delete</c> followed by the singular resource label.
    /// </value>
    public Func<ResourceLabelContext, string> ConfirmLabel { get; set; } =
        resource => $"Delete {resource.SingularLabel}";

    /// <summary>
    ///     Gets or sets the callback that generates the delete confirmation's default message.
    /// </summary>
    /// <value>
    ///     The default returns a question with the singular resource label, such as
    ///     <c>Are you sure you want to delete this product?</c>.
    /// </value>
    public Func<ResourceLabelContext, string> Message { get; set; } =
        resource => $"Are you sure you want to delete this {resource.SingularLabel}?";

    /// <summary>
    ///     Gets or sets the callback that generates the delete confirmation's default title.
    /// </summary>
    /// <value>
    ///     The default returns <c>Delete</c> followed by the singular resource label.
    /// </value>
    public Func<ResourceLabelContext, string> Title { get; set; } =
        resource => $"Delete {resource.SingularLabel}";
}
