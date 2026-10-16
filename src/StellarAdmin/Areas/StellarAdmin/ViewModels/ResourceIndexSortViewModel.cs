using StellarAdmin.UI;

namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The sort applied to an index page, including a configured default sort.
/// </summary>
public sealed class ResourceIndexSortViewModel
{
    /// <summary>
    ///     The field the list is sorted by, or <c>null</c> when the list is unsorted.
    /// </summary>
    public string? By { get; }

    /// <summary>
    ///     The direction the list is sorted in. Meaningful only when <see cref="By" /> is set.
    /// </summary>
    public DataGridSortDirection Direction { get; }

    internal ResourceIndexSortViewModel(string? by, DataGridSortDirection direction)
    {
        By = by;
        Direction = direction;
    }
}
