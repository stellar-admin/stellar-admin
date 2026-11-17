namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a resource's edit form.
/// </summary>
public abstract class ResourceEditOptions : ResourceFormOptions
{
    /// <summary>
    ///     Gets the configuration of the inline row that edits resources.
    /// </summary>
    /// <value>
    ///     The configuration, or <see langword="null" /> when resources are edited on the edit
    ///     page.
    /// </value>
    public ResourceInlineEditOptions? InlineRow { get; internal set; }

    /// <summary>
    ///     Gets the type of the model the edit form binds to.
    /// </summary>
    public abstract Type ModelType { get; }

    // The page's record summary: its own, or once checked, the resource's when the form edits the resource
    internal ResourceSummaryOptions? Summary { get; set; }
}
