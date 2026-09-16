namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The empty state of an index page.
/// </summary>
/// <param name="Icon">The name of the icon rendered when the list is empty.</param>
/// <param name="Title">The title rendered when the list is empty.</param>
/// <param name="Description">The description rendered when the list is empty.</param>
public sealed record ResourceIndexEmptyViewModel(string Icon, string Title, string Description);
