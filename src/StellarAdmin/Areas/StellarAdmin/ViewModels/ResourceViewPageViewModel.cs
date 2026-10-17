using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     Represents the model of the view page.
/// </summary>
/// <remarks>
///     The model isn't generic, so a view works for any resource. HTML helpers read field values
///     and metadata from the runtime type of <see cref="Entity" />.
/// </remarks>
public sealed class ResourceViewPageViewModel
{
    /// <summary>
    ///     Gets the page's delete confirmation dialog, or <see langword="null" /> when the
    ///     resource can't be deleted.
    /// </summary>
    public ResourceFormDeleteDialogViewModel? Delete { get; internal init; }

    /// <summary>
    ///     Gets the data that the field displays prepared for the current request.
    /// </summary>
    public IReadOnlyDictionary<FormDisplayFieldOptions, object?> DisplayData
    {
        get;
        internal init;
    } = new Dictionary<FormDisplayFieldOptions, object?>();

    /// <summary>
    ///     Gets the label of the page's edit link, or <see langword="null" /> when the resource
    ///     can't be edited.
    /// </summary>
    public string? EditLabel { get; internal init; }

    /// <summary>
    ///     Gets the resource or model that the page shows.
    /// </summary>
    /// <remarks>
    ///     An overridden view can cast it to the application's type.
    /// </remarks>
    public object Entity { get; internal init; } = null!;

    /// <summary>
    ///     Gets the key of the resource that the page shows.
    /// </summary>
    public string EntityId { get; internal init; } = "";

    /// <summary>
    ///     Gets all of the page's fields in display order, including the fields inside sections
    ///     and groups.
    /// </summary>
    public IReadOnlyList<FormDisplayFieldOptions> Fields { get; internal init; } = [];

    /// <summary>
    ///     Gets the page's top-level fields and containers.
    /// </summary>
    /// <remarks>
    ///     Containers such as sections and groups hold their own items.
    /// </remarks>
    public IReadOnlyList<FormItemOptions> Items { get; internal init; } = [];

    /// <summary>
    ///     Gets the layout of the page's sections, or <see langword="null" /> to use the
    ///     app-wide default.
    /// </summary>
    public FormSectionLayout? SectionLayout { get; internal init; }

    /// <summary>
    ///     Gets the page title.
    /// </summary>
    public string Title { get; internal init; } = "";

    internal FormGridColumnDefinitions Columns { get; init; }

    internal ResourceViewPageViewModel() { }
}
