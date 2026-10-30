using StellarAdmin.Diagnostics;
using StellarAdmin.Widgets;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a resource's index page.
/// </summary>
public sealed class ResourceIndexOptions
{
    /// <summary>
    ///     Gets the label of the cancel button on an inline row.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use
    ///     <see cref="ResourceIndexLabelOptions.CancelLabel" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? CancelLabel { get; internal set; }

    /// <summary>
    ///     Gets the columns, in display order.
    /// </summary>
    public IReadOnlyList<DataGridColumnOptions> Columns => MutableColumns;

    /// <summary>
    ///     Gets the label of the create button.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use
    ///     <see cref="ResourceIndexLabelOptions.CreateLabel" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? CreateLabel { get; internal set; }

    /// <summary>
    ///     Gets the message of the toast shown when a resource is added in a new row or saved with
    ///     the add another button.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use
    ///     <see cref="ResourceIndexLabelOptions.CreatedMessage" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? CreatedMessage { get; internal set; }

    /// <summary>
    ///     Gets the sort order used when the user hasn't sorted the list.
    /// </summary>
    /// <value>
    ///     The sort order, or <see langword="null" /> to use the data source's order. The default
    ///     is <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     The sort order must select a sortable column.
    /// </remarks>
    public ResourceSort? DefaultSort { get; internal set; }

    /// <summary>
    ///     Gets the label of the delete button.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use
    ///     <see cref="ResourceIndexLabelOptions.DeleteLabel" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? DeleteLabel { get; internal set; }

    /// <summary>
    ///     Gets the label of the edit link.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use
    ///     <see cref="ResourceIndexLabelOptions.EditLabel" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? EditLabel { get; internal set; }

    /// <summary>
    ///     Gets the description of the toast shown when a saved inline row isn't on the
    ///     reloaded page.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use
    ///     <see cref="ResourceIndexLabelOptions.NotShownMessage" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? NotShownMessage { get; internal set; }

    /// <summary>
    ///     Gets the paging configuration.
    /// </summary>
    /// <value>
    ///     The configuration, or <see langword="null" /> when the list isn't paged. The default is
    ///     <see langword="null" />.
    /// </value>
    public ResourcePagingOptions? Paging { get; internal set; }

    /// <summary>
    ///     Gets the configuration of rows that expand to show details.
    /// </summary>
    /// <value>
    ///     The configuration, or <see langword="null" /> when rows don't expand. The default is
    ///     <see langword="null" />.
    /// </value>
    public ResourceRowDetailsOptions? RowDetails { get; internal set; }

    /// <summary>
    ///     Gets the label of the save button on an inline row.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use
    ///     <see cref="ResourceIndexLabelOptions.SaveLabel" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? SaveLabel { get; internal set; }

    /// <summary>
    ///     Gets the message of the toast shown when an edited inline row isn't on the
    ///     reloaded page.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use
    ///     <see cref="ResourceIndexLabelOptions.SavedMessage" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? SavedMessage { get; internal set; }

    /// <summary>
    ///     Gets the scopes configuration.
    /// </summary>
    /// <value>
    ///     The configuration, or <see langword="null" /> when the list has no scopes. The default
    ///     is <see langword="null" />.
    /// </value>
    public ResourceScopesOptions? Scopes { get; internal set; }

    /// <summary>
    ///     Gets the search configuration.
    /// </summary>
    /// <value>
    ///     The configuration, or <see langword="null" /> when the list can't be searched. The
    ///     default is <see langword="null" />.
    /// </value>
    public ResourceSearchOptions? Search { get; internal set; }

    /// <summary>
    ///     Gets the page title.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use
    ///     <see cref="ResourceIndexLabelOptions.Title" />. The default is <see langword="null" />.
    /// </value>
    public string? Title { get; internal set; }

    /// <summary>
    ///     Gets the label of the view link.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use
    ///     <see cref="ResourceIndexLabelOptions.ViewLabel" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? ViewLabel { get; internal set; }

    internal ConfigurationSource? DefaultSortSource { get; set; }

    internal List<DataGridColumnOptions> MutableColumns { get; } = [];

    // The widgets as configured, then, once checked, those that can be shown
    internal List<WidgetOptions> Widgets { get; } = [];
}
