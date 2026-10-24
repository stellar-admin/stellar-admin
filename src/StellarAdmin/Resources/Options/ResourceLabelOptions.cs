namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the default text of resource pages.
/// </summary>
public sealed class ResourceLabelOptions
{
    /// <summary>
    ///     Gets the default text of the create page.
    /// </summary>
    public ResourceCreateLabelOptions Create { get; } = new();

    /// <summary>
    ///     Gets the default text of the delete confirmation.
    /// </summary>
    public ResourceDeleteLabelOptions Delete { get; } = new();

    /// <summary>
    ///     Gets the default text of dictionary editors.
    /// </summary>
    public DictionaryEditorLabelOptions DictionaryEditor { get; } = new();

    /// <summary>
    ///     Gets the default text of the edit page.
    /// </summary>
    public ResourceEditLabelOptions Edit { get; } = new();

    /// <summary>
    ///     Gets the default text of the index page.
    /// </summary>
    public ResourceIndexLabelOptions Index { get; } = new();

    /// <summary>
    ///     Gets the default text of list editors.
    /// </summary>
    public ListEditorLabelOptions ListEditor { get; } = new();

    /// <summary>
    ///     Gets the default text of lookup editors.
    /// </summary>
    public LookupLabelOptions Lookup { get; } = new();

    /// <summary>
    ///     Gets the default text of the sheet that pages load content into.
    /// </summary>
    public ResourceSheetLabelOptions Sheet { get; } = new();

    /// <summary>
    ///     Gets the default text of the view page.
    /// </summary>
    public ResourceViewLabelOptions View { get; } = new();
}
