namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Default edit page text.
/// </summary>
public sealed class ResourceEditLabelOptions
{
    /// <summary>
    ///     The callback that generates the default edit form submit label.
    /// </summary>
    public Func<ResourceLabelContext, string> SubmitLabel { get; set; } =
        resource => $"Save {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default edit page title.
    /// </summary>
    public Func<ResourceLabelContext, string> Title { get; set; } =
        resource => $"Edit {resource.SingularLabel}";
}
