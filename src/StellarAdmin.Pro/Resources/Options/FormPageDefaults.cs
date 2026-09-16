namespace StellarAdmin.Pro.Resources.Options;

/// <summary>
///     The library defaults for a form page.
/// </summary>
/// <param name="Title">The page title.</param>
/// <param name="SubmitLabel">The label of the submit button.</param>
/// <param name="Fields">
///     The properties that seed the fields. Properties the entity does not have are skipped.
/// </param>
public sealed record FormPageDefaults(string Title, string SubmitLabel, string[] Fields);
