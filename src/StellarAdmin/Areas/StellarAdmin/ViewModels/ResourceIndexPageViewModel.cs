using System.Collections;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     Represents the model of an index page.
/// </summary>
/// <typeparam name="TResource">The type of the resource.</typeparam>
public sealed class ResourceIndexPageViewModel<TResource> : IResourceIndexPageViewModel
{
    /// <inheritdoc />
    public bool CanCreate { get; init; }

    /// <inheritdoc />
    public bool CanEdit { get; init; }

    /// <inheritdoc />
    public bool CanView { get; init; }

    /// <inheritdoc />
    public required IReadOnlyList<DataGridColumnOptions> Columns { get; init; }

    /// <inheritdoc />
    public required string CreateLabel { get; init; }

    /// <inheritdoc />
    public ResourceIndexDeleteDialogViewModel? Delete { get; init; }

    /// <inheritdoc />
    public string? DeleteLabel { get; init; }

    /// <inheritdoc />
    public string? EditLabel { get; init; }

    /// <inheritdoc />
    public ResourceIndexInlineRowsViewModel? InlineRows { get; init; }

    /// <summary>
    ///     Gets the resources on the current page.
    /// </summary>
    /// <remarks>
    ///     Empty when the shown table's handler loads its rows; <see cref="IResourceIndexPageViewModel.Items" />
    ///     has them.
    /// </remarks>
    public required IReadOnlyList<TResource> Items { get; init; }

    /// <inheritdoc />
    public IReadOnlyList<ResourceIndexDataLayoutViewModel> Layouts { get; init; } = [];

    /// <summary>
    ///     Gets a delegate that returns the key of a resource, or <see langword="null" /> when the
    ///     resource has no key.
    /// </summary>
    public Func<TResource, string>? KeySelector { get; init; }

    /// <inheritdoc />
    public ResourceIndexPagingViewModel? Paging { get; init; }

    /// <inheritdoc />
    public ResourceIndexPartialDataLayoutViewModel? PartialLayout { get; init; }

    /// <inheritdoc />
    public ResourceIndexQuery Query { get; init; } = new();

    /// <inheritdoc />
    public ResourceRowDetailsOptions? RowDetails { get; init; }

    /// <inheritdoc />
    public bool RowsOnly { get; init; }

    /// <inheritdoc />
    public IReadOnlyList<ResourceIndexScopeViewModel> Scopes { get; init; } = [];

    /// <inheritdoc />
    public ResourceIndexSearchViewModel? Search { get; init; }

    /// <inheritdoc />
    public ResourceIndexSortViewModel? Sort { get; init; }

    /// <inheritdoc />
    public required string Title { get; init; }

    /// <inheritdoc />
    public string? ViewLabel { get; init; }

    IEnumerable IResourceIndexPageViewModel.Items => (IEnumerable?)HandlerRows ?? Items;

    // The rows a table's handler loads, or null when the table shows the resources
    internal IReadOnlyList<object>? HandlerRows { get; init; }

    // The key of a row's record, or null to use KeySelector
    internal Func<object, string?>? RowKey { get; init; }

    // The URL of the view page a row links to
    internal Func<object, string?>? ViewUrl { get; init; }

    internal ResourceIndexPageViewModel() { }

    // A resource without a key has no keys, such as for the cards of a resource without UseKey
    string? IResourceIndexPageViewModel.GetKey(object resource) =>
        RowKey is { } rowKey ? rowKey(resource) : KeySelector?.Invoke((TResource)resource);

    string? IResourceIndexPageViewModel.GetViewUrl(object resource) => ViewUrl?.Invoke(resource);
}
