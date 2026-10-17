using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     Represents the model of the create and edit pages.
/// </summary>
/// <remarks>
///     The model isn't generic, so a view works for any resource. HTML helpers read field values
///     and metadata from the runtime type of <see cref="Entity" />.
/// </remarks>
public sealed class ResourceFormPageViewModel
{
    /// <summary>
    ///     Represents the prefix of the form's input names.
    /// </summary>
    /// <remarks>
    ///     A posted field is named <c>{BindingPrefix}.{FieldName}</c>.
    /// </remarks>
    public const string BindingPrefix = nameof(Entity);

    /// <summary>
    ///     Gets the page's delete confirmation dialog, or <see langword="null" /> when the
    ///     resource can't be deleted.
    /// </summary>
    public ResourceFormDeleteDialogViewModel? Delete { get; internal init; }

    /// <summary>
    ///     Gets the resource or create model that the form edits.
    /// </summary>
    /// <remarks>
    ///     An overridden view can cast it to the application's type.
    /// </remarks>
    public object Entity { get; internal init; } = null!;

    /// <summary>
    ///     Gets all of the form's fields in display order, including the fields inside sections
    ///     and groups.
    /// </summary>
    public IReadOnlyList<FormFieldOptions> Fields { get; internal init; } = [];

    /// <summary>
    ///     Gets the form's top-level fields and containers.
    /// </summary>
    /// <remarks>
    ///     Containers such as sections and groups hold their own items.
    /// </remarks>
    public IReadOnlyList<FormItemOptions> Items { get; internal init; } = [];

    /// <summary>
    ///     Gets the data that the field displays prepared for the current request.
    /// </summary>
    public IReadOnlyDictionary<FormDisplayFieldOptions, object?> DisplayData
    {
        get;
        internal set;
    } = new Dictionary<FormDisplayFieldOptions, object?>();

    /// <summary>
    ///     Gets the data that the field editors prepared for the current request, by field name.
    /// </summary>
    public IReadOnlyDictionary<string, object?> EditorData { get; internal set; } =
        new Dictionary<string, object?>();

    /// <summary>
    ///     Gets the templates that the field editors use, by field name.
    /// </summary>
    public IReadOnlyDictionary<string, string> EditorTemplates { get; internal set; } =
        new Dictionary<string, string>();

    /// <summary>
    ///     Gets the layout of the form's sections, or <see langword="null" /> to use the
    ///     app-wide default.
    /// </summary>
    public FormSectionLayout? SectionLayout { get; internal init; }

    /// <summary>
    ///     Gets the label of the form's submit button.
    /// </summary>
    public string SubmitLabel { get; internal init; } = "";

    /// <summary>
    ///     Gets the page subtitle, or <see langword="null" /> for none.
    /// </summary>
    public string? Subtitle { get; internal init; }

    /// <summary>
    ///     Gets the page title.
    /// </summary>
    public string Title { get; internal init; } = "";

    internal FormGridColumnDefinitions Columns { get; init; }

    internal ResourceFormPageViewModel() { }
}
