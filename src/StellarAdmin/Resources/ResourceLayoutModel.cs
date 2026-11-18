namespace StellarAdmin.Resources;

/// <summary>
///     Represents the model of a partial view that renders a layout of a resource's index page.
/// </summary>
/// <typeparam name="TItem">The type of the items the layout shows.</typeparam>
public sealed class ResourceLayoutModel<TItem>
{
    private readonly Func<TItem, string?> _editUrl;
    private readonly Func<object?, string> _indexUrl;
    private readonly Func<TItem, string?> _viewUrl;

    /// <summary>
    ///     Gets the items the layout shows.
    /// </summary>
    /// <remarks>
    ///     A partial layout receives every resource the scope and search select, or every item its
    ///     handler returns; it has no paging.
    /// </remarks>
    public IReadOnlyList<TItem> Items { get; }

    /// <summary>
    ///     Gets the request that listed the items, with the layout's key, scope and search.
    /// </summary>
    /// <remarks>
    ///     A scope or search that the layout ignores is <see langword="null" /> here.
    /// </remarks>
    public ResourceListRequest Request { get; }

    internal ResourceLayoutModel(
        IReadOnlyList<TItem> items,
        ResourceListRequest request,
        Func<TItem, string?> viewUrl,
        Func<TItem, string?> editUrl,
        Func<object?, string> indexUrl
    )
    {
        Items = items;
        Request = request;
        _viewUrl = viewUrl;
        _editUrl = editUrl;
        _indexUrl = indexUrl;
    }

    /// <summary>
    ///     Returns the URL of an item's edit page.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>
    ///     The URL, or <see langword="null" /> when the resource has no edit page or the items come
    ///     from a handler.
    /// </returns>
    public string? GetEditUrl(TItem item) => _editUrl(item);

    /// <summary>
    ///     Returns the URL of the index page as it shows, with the current layout, scope and search.
    /// </summary>
    /// <param name="values">
    ///     An object whose properties add query string values or replace the current ones, such as
    ///     <c>new { layout = "catalog" }</c>, or <see langword="null" /> to add none.
    /// </param>
    /// <returns>The URL.</returns>
    public string GetIndexUrl(object? values = null) => _indexUrl(values);

    /// <summary>
    ///     Returns the URL of an item's view page.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>
    ///     The URL, or <see langword="null" /> when the resource has no view page or the items come
    ///     from a handler.
    /// </returns>
    public string? GetViewUrl(TItem item) => _viewUrl(item);
}
