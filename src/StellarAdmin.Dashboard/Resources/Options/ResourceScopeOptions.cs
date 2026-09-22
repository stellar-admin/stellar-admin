namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     A named resource filter.
/// </summary>
/// <param name="Id">The identifier passed to the data source.</param>
/// <param name="Title">The scope tab label.</param>
public sealed record ResourceScopeOptions(string Id, string Title);
