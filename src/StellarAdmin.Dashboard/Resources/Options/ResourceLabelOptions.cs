namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Default page text for resources.
/// </summary>
public sealed class ResourceLabelOptions
{
    /// <summary>
    ///     The callback that generates the default create form submit label.
    /// </summary>
    public Func<ResourceLabelContext, string> CreateSubmitLabel { get; set; } =
        resource => $"Create {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default create page title.
    /// </summary>
    public Func<ResourceLabelContext, string> CreateTitle { get; set; } =
        resource => $"Create {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default edit form submit label.
    /// </summary>
    public Func<ResourceLabelContext, string> EditSubmitLabel { get; set; } =
        resource => $"Save {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default edit page title.
    /// </summary>
    public Func<ResourceLabelContext, string> EditTitle { get; set; } =
        resource => $"Edit {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default index create button label.
    /// </summary>
    public Func<ResourceLabelContext, string> IndexCreateLabel { get; set; } = resource => "Create";

    /// <summary>
    ///     The callback that generates the default index edit link label.
    /// </summary>
    public Func<ResourceLabelContext, string> IndexEditLabel { get; set; } =
        resource => $"Edit {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default index page title.
    /// </summary>
    public Func<ResourceLabelContext, string> IndexTitle { get; set; } =
        resource => resource.PluralLabel;
}
