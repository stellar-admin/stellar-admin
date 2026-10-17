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
    public required IReadOnlyList<TResource> Items { get; init; }

    /// <summary>
    ///     Gets a delegate that returns the key of a resource, or <see langword="null" /> when the
    ///     resource has no key.
    /// </summary>
    public Func<TResource, string>? KeySelector { get; init; }

    /// <inheritdoc />
    public ResourceIndexPagingViewModel? Paging { get; init; }

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

    IEnumerable IResourceIndexPageViewModel.Items => Items;

    internal ResourceIndexPageViewModel() { }

    string IResourceIndexPageViewModel.GetKey(object resource) => KeySelector!((TResource)resource);
}
