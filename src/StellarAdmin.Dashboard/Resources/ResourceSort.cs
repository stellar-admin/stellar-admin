namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     The field and direction used to order resources.
/// </summary>
public sealed record ResourceSort(string Field, ResourceSortDirection Direction);
