using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The view model of the view page. Deliberately non-generic so the views need no knowledge of the entity
///     type; the stock HTML helpers resolve field values and metadata from the runtime type of
///     <see cref="Entity" />.
/// </summary>
public sealed class ResourceViewPageViewModel
{
    /// <summary>
    ///     The delete confirmation of the page, or <c>null</c> when the page cannot delete the entity.
    /// </summary>
    public ResourceFormDeleteDialogViewModel? Delete { get; internal init; }

    /// <summary>
    ///     Request data prepared by the configured field displays.
    /// </summary>
    public IReadOnlyDictionary<FormDisplayFieldOptions, object?> DisplayData
    {
        get;
        internal init;
    } = new Dictionary<FormDisplayFieldOptions, object?>();

    /// <summary>
    ///     The label of the page's edit link, or <c>null</c> when the entity cannot be edited.
    /// </summary>
    public string? EditLabel { get; internal init; }

    /// <summary>The entity the page displays. An overridden view can cast this to the application's type.</summary>
    public object Entity { get; internal init; } = null!;

    /// <summary>The key of the entity the page displays.</summary>
    public string EntityId { get; internal init; } = "";

    /// <summary>
    ///     All fields as a flat list, including fields inside sections and groups, in display order.
    /// </summary>
    public IReadOnlyList<FormDisplayFieldOptions> Fields { get; internal init; } = [];

    /// <summary>
    ///     The page's top-level fields and containers, preserving nested sections and groups for rendering.
    /// </summary>
    public IReadOnlyList<FormItemOptions> Items { get; internal init; } = [];

    /// <summary>
    ///     The layout of the page's sections.
    /// </summary>
    /// <remarks>
    ///     Defaults to the application setting.
    /// </remarks>
    public FormSectionLayout? SectionLayout { get; internal init; }

    /// <summary>The page title.</summary>
    public string Title { get; internal init; } = "";

    internal FormGridColumnDefinitions Columns { get; init; }
}
