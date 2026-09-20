namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures a resource's create page.
/// </summary>
public sealed class ResourceCreateOptions<TResource>
{
    /// <summary>
    ///     The factory used to instantiate a resource for the create form.
    /// </summary>
    /// <remarks>
    ///     When null, instances are created using the resource's parameterless constructor.
    /// </remarks>
    public Func<TResource>? Factory { get; set; }

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

    /// <summary>
    ///     Creates a resource using the configured factory or its parameterless constructor.
    /// </summary>
    public TResource CreateInstance() =>
        Factory is { } factory ? factory() : Activator.CreateInstance<TResource>()!;
}
