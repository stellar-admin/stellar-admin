using System.Collections;
using StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The resources and presentation settings for an index page.
/// </summary>
public sealed class ResourceIndexPageViewModel<TResource> : IResourceIndexPageViewModel
{
    /// <summary>
    ///     Whether resources can be created.
    /// </summary>
    public bool CanCreate { get; init; }

    /// <summary>
    ///     Whether resources can be edited.
    /// </summary>
    public bool CanEdit { get; init; }

    /// <summary>
    ///     The columns displayed in order.
    /// </summary>
    public required IReadOnlyList<DataGridColumnOptions> Columns { get; init; }

    /// <summary>
    ///     The create button label.
    /// </summary>
    public required string CreateLabel { get; init; }

    /// <summary>
    ///     The delete confirmation dialog.
    /// </summary>
    public ResourceIndexDeleteDialogViewModel? Delete { get; init; }

    /// <summary>
    ///     The delete button label.
    /// </summary>
    public string? DeleteLabel { get; init; }

    /// <summary>
    ///     The edit link label.
    /// </summary>
    public string? EditLabel { get; init; }

    /// <summary>
    ///     The resources to display.
    /// </summary>
    public required IReadOnlyList<TResource> Items { get; init; }

    /// <summary>
    ///     Returns the key used in resource links.
    /// </summary>
    public Func<TResource, string>? KeySelector { get; init; }

    /// <summary>
    ///     The paging controls, or null when paging is disabled.
    /// </summary>
    public ResourceIndexPagingViewModel? Paging { get; init; }

    /// <summary>
    ///     The index settings explicitly supplied in the query string.
    /// </summary>
    public ResourceIndexQuery Query { get; init; } = new();

    /// <summary>
    ///     The search box, or null when searching is disabled.
    /// </summary>
    public ResourceIndexSearchViewModel? Search { get; init; }

    /// <summary>
    ///     The selected ordering.
    /// </summary>
    public ResourceIndexSortViewModel? Sort { get; init; }

    /// <summary>
    ///     The page title.
    /// </summary>
    public required string Title { get; init; }

    IEnumerable IResourceIndexPageViewModel.Items => Items;

    string IResourceIndexPageViewModel.GetKey(object resource) => KeySelector!((TResource)resource);
}
