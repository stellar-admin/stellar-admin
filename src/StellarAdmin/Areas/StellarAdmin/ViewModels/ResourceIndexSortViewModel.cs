using StellarAdmin.UI;

namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     Represents the sort order of an index page, including a default sort order.
/// </summary>
public sealed class ResourceIndexSortViewModel
{
    /// <summary>
    ///     Gets the field that the list is sorted by, or <see langword="null" /> when the list is
    ///     unsorted.
    /// </summary>
    public string? By { get; }

    /// <summary>
    ///     Gets the direction that the list is sorted in.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the sort direction. It applies only when
    ///     <see cref="By" /> is set.
    /// </value>
    public DataGridSortDirection Direction { get; }

    internal ResourceIndexSortViewModel(string? by, DataGridSortDirection direction)
    {
        By = by;
        Direction = direction;
    }
}
