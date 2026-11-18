using StellarAdmin.Diagnostics;
using StellarAdmin.Widgets;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a resource's index page.
/// </summary>
public sealed class ResourceIndexOptions
{
    // The key of the table layout that the one-argument Index overload configures
    internal const string ImplicitTableKey = "table";

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
    ///     Gets the layouts, in the order the user can switch between them.
    /// </summary>
    /// <remarks>
    ///     The first layout is the one the page shows by default.
    /// </remarks>
    public IReadOnlyList<ResourceLayoutOptions> Layouts => MutableLayouts;

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

    internal List<ResourceLayoutOptions> MutableLayouts { get; } = [];

    // Whether the two-argument Index overload declared the layouts
    internal bool HasDeclaredLayouts { get; private set; }

    // The tables that show the resource's own rows, in layout order. The first one has the inline rows, and lookups and
    // references sort by its default sort
    internal IEnumerable<ResourceTableLayoutOptions> RecordTables =>
        Tables.Where(table => table.Loader is null);

    // The tables, in layout order
    internal IEnumerable<ResourceTableLayoutOptions> Tables =>
        MutableLayouts.OfType<ResourceTableLayoutOptions>();

    // The widgets as configured, then, once checked, those that can be shown
    internal List<WidgetOptions> Widgets { get; } = [];

    // The layout the page shows for a key, or the first layout when the key matches none
    internal ResourceLayoutOptions? FindLayout(string? key) =>
        MutableLayouts.FirstOrDefault(layout =>
            string.Equals(layout.Key, key, StringComparison.OrdinalIgnoreCase)
        ) ?? MutableLayouts.FirstOrDefault();

    // The layouts route replaces the implicit table that an earlier one-argument Index call configured, such as a
    // default an application overrides
    internal void DeclareLayouts()
    {
        if (!HasDeclaredLayouts)
        {
            MutableLayouts.Clear();
            HasDeclaredLayouts = true;
        }
    }

    internal void AddPartial(string key, string partialName, ResourceLayoutLoader? loader = null) =>
        MutableLayouts.Add(new ResourcePartialLayoutOptions(key, partialName, loader));

    // The layout with a key, which its Add call has already added
    internal ResourceLayoutOptions GetLayout(string key) =>
        MutableLayouts.First(layout => layout.Key == key);

    internal ResourceTableLayoutOptions GetOrAddTable(string key)
    {
        if (MutableLayouts.Find(layout => layout.Key == key) is ResourceTableLayoutOptions table)
        {
            return table;
        }

        table = new(key);
        MutableLayouts.Add(table);

        return table;
    }
}
