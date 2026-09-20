namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures a resource's create page.
/// </summary>
public sealed class ResourceCreateOptions
{
    /// <summary>
    ///     The fields displayed in order.
    /// </summary>
    public IList<FormFieldOptions> Fields { get; } = new List<FormFieldOptions>();

    /// <summary>
    ///     The submit button label.
    /// </summary>
    public string? SubmitLabel { get; set; }

    /// <summary>
    ///     The page title.
    /// </summary>
    public string? Title { get; set; }
}
