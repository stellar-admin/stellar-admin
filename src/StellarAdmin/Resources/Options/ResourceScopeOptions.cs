namespace StellarAdmin.Resources.Options;

/// <summary>
///     Describes a scope, which is shown as a tab that filters a resource's index page.
/// </summary>
/// <param name="Id">The identifier passed to the data source when the scope is selected.</param>
/// <param name="Title">The label of the scope's tab.</param>
public record ResourceScopeOptions(string Id, string Title);
