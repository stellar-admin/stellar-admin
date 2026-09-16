namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     The library defaults for an index page.
/// </summary>
/// <param name="Title">The page title.</param>
/// <param name="CreateLabel">The label of the button that opens the create page.</param>
/// <param name="EmptyTitle">The title rendered when the list is empty.</param>
/// <param name="EmptyDescription">The description rendered when the list is empty.</param>
/// <param name="EmptyIcon">The name of the icon rendered when the list is empty.</param>
/// <param name="Columns">
///     The properties that seed the columns. Properties the entity does not have are skipped.
/// </param>
/// <param name="SortBy">The property to sort by, or <c>null</c> for no default sort.</param>
public sealed record IndexPageDefaults(
    string Title,
    string CreateLabel,
    string EmptyTitle,
    string EmptyDescription,
    string EmptyIcon,
    string[] Columns,
    string? SortBy
);
