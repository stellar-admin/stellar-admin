namespace StellarAdmin.Resources;

/// <summary>
///     Specifies how resources are sorted.
/// </summary>
/// <param name="Field">The name of the field to sort by.</param>
/// <param name="Direction">One of the enumeration values that specifies the sort direction.</param>
public sealed record ResourceSort(string Field, ResourceSortDirection Direction);
