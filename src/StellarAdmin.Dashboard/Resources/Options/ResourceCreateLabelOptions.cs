namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Default create page text.
/// </summary>
public sealed class ResourceCreateLabelOptions
{
    /// <summary>
    ///     The callback that generates the default create form submit label.
    /// </summary>
    public Func<ResourceLabelContext, string> SubmitLabel { get; set; } =
        resource => $"Create {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default create page title.
    /// </summary>
    public Func<ResourceLabelContext, string> Title { get; set; } =
        resource => $"Create {resource.SingularLabel}";
}
