namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     A request for a page of lookup items.
/// </summary>
/// <param name="Term">The search text, or null to list all items.</param>
/// <param name="Skip">The number of items to skip.</param>
/// <param name="Take">The maximum number of items to return.</param>
public sealed record LookupQuery(string? Term, int Skip, int Take);
