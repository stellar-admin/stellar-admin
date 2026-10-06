namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Default delete confirmation text.
/// </summary>
public sealed class ResourceDeleteLabelOptions
{
    /// <summary>
    ///     The callback that generates the default delete cancellation label.
    /// </summary>
    public Func<ResourceLabelContext, string> CancelLabel { get; set; } = resource => "Cancel";

    /// <summary>
    ///     The callback that generates the default delete confirmation button label.
    /// </summary>
    public Func<ResourceLabelContext, string> ConfirmLabel { get; set; } =
        resource => $"Delete {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default delete confirmation message.
    /// </summary>
    public Func<ResourceLabelContext, string> Message { get; set; } =
        resource => $"Are you sure you want to delete this {resource.SingularLabel}?";

    /// <summary>
    ///     The callback that generates the default delete confirmation title.
    /// </summary>
    public Func<ResourceLabelContext, string> Title { get; set; } =
        resource => $"Delete {resource.SingularLabel}";
}
