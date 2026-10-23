namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Supplies the items of a lookup editor.
/// </summary>
/// <remarks>
///     Integrations derive from this class to search their own data.
/// </remarks>
public abstract class LookupItems
{
    /// <summary>
    ///     Gets a value that indicates whether the items have a description.
    /// </summary>
    public abstract bool HasDescription { get; }

    /// <summary>
    ///     Gets the type of the items.
    /// </summary>
    /// <value>
    ///     The type, or <see langword="null" /> when it is unknown. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     When a lookup editor lets users create items, the resource registered for this type
    ///     creates them, unless the editor names a resource.
    /// </remarks>
    public virtual Type? ItemType => null;

    /// <summary>
    ///     Gets the type of media the items display.
    /// </summary>
    /// <value>
    ///     The media type, such as <see cref="ItemMedia.Avatar" />, or <see langword="null" /> when
    ///     the items have no media.
    /// </value>
    /// <remarks>
    ///     The placeholders shown while items load take the shape of this media.
    /// </remarks>
    public abstract Type? MediaType { get; }

    /// <summary>
    ///     Gets the type of the items' values.
    /// </summary>
    /// <value>
    ///     The type of the values, or <see langword="null" /> when it isn't known. The default is
    ///     <see langword="null" />. A field whose property doesn't hold this type is a
    ///     configuration problem, and keeps its default editor.
    /// </value>
    public virtual Type? ValueType => null;

    /// <summary>
    ///     Returns the selected items for the field's current value.
    /// </summary>
    /// <param name="services">The request's service provider.</param>
    /// <param name="context">The context of the field, which holds its current value.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    ///     The selected items in the order of the values, or an empty list when nothing is
    ///     selected.
    /// </returns>
    /// <remarks>
    ///     The field's value is one value or a collection of values. Format each item's value in
    ///     the current culture.
    /// </remarks>
    public abstract Task<IReadOnlyList<ChoiceItem>> FindAsync(
        IServiceProvider services,
        FieldEditorContext context,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///     Returns a page of items that match a query.
    /// </summary>
    /// <param name="services">The request's service provider.</param>
    /// <param name="query">The search term and the page of items to return.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The matching items, and whether more items follow them.</returns>
    public abstract Task<LookupResults> SearchAsync(
        IServiceProvider services,
        LookupQuery query,
        CancellationToken cancellationToken
    );
}
