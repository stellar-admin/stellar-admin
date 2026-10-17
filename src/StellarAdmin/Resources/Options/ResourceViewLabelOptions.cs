namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the default text of the view page.
/// </summary>
public sealed class ResourceViewLabelOptions
{
    /// <summary>
    ///     Gets or sets the delegate that returns the default title of the view page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "View" followed by
    ///     the resource's singular label.
    /// </value>
    public Func<ResourceLabelContext, string> Title { get; set; } =
        resource => $"View {resource.SingularLabel}";
}
