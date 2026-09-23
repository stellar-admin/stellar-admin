using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The view model of the form pages (create and edit). Deliberately non-generic so
///     the views need no knowledge of the entity type; the stock HTML helpers resolve
///     field values and metadata from the runtime type of <see cref="Entity" />.
/// </summary>
public class ResourceFormPageViewModel
{
    /// <summary>
    ///     The prefix the form's input names carry. A posted field is named
    ///     "{BindingPrefix}.{FieldName}".
    /// </summary>
    public const string BindingPrefix = nameof(Entity);

    /// <summary>
    ///     The delete confirmation of the page, or <c>null</c> when the page cannot delete
    ///     the entity.
    /// </summary>
    public ResourceFormDeleteDialogViewModel? Delete { get; internal init; }

    /// <summary>The entity the form renders. An overridden view can cast this to the application's type.</summary>
    public object Entity { get; internal init; } = null!;

    /// <summary>
    ///     All form fields as a flat list, including fields inside sections, groups, and rows, in display order.
    /// </summary>
    public IReadOnlyList<FormFieldOptions> Fields { get; internal init; } = [];

    /// <summary>
    ///     The form's top-level fields and containers, preserving nested sections, groups, and rows for rendering.
    /// </summary>
    public IReadOnlyList<FormItemOptions> Items { get; internal init; } = [];

    /// <summary>
    ///     Request data prepared by the configured field editors.
    /// </summary>
    public IReadOnlyDictionary<string, object?> EditorData { get; internal set; } =
        new Dictionary<string, object?>();

    /// <summary>
    ///     The layout of the form sections.
    /// </summary>
    /// <remarks>
    ///     Defaults to the application setting.
    /// </remarks>
    public FormSectionLayout? SectionLayout { get; internal init; }

    /// <summary>The label of the form's submit button.</summary>
    public string SubmitLabel { get; internal init; } = "";

    /// <summary>The page subtitle, or <c>null</c> to render no subtitle.</summary>
    public string? Subtitle { get; internal init; }

    /// <summary>The page title.</summary>
    public string Title { get; internal init; } = "";
}
