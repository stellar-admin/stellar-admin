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
    ///     The columns displayed in order.
    /// </summary>
    public required IReadOnlyList<DataGridColumnOptions> Columns { get; init; }

    /// <summary>
    ///     The create button label.
    /// </summary>
    public required string CreateLabel { get; init; }

    /// <summary>
    ///     The resources to display.
    /// </summary>
    public required IReadOnlyList<TResource> Items { get; init; }

    /// <summary>
    ///     The page title.
    /// </summary>
    public required string Title { get; init; }

    IEnumerable IResourceIndexPageViewModel.Items => Items;
}
