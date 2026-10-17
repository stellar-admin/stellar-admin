namespace StellarAdmin.Resources;

/// <summary>
///     Provides the resource labels that page text can use.
/// </summary>
/// <param name="singularLabel">The singular label of the resource.</param>
/// <param name="pluralLabel">The plural label of the resource.</param>
public sealed class ResourceLabelContext(string singularLabel, string pluralLabel)
{
    /// <summary>
    ///     Gets the plural label of the resource.
    /// </summary>
    public string PluralLabel { get; } = pluralLabel;

    /// <summary>
    ///     Gets the singular label of the resource.
    /// </summary>
    public string SingularLabel { get; } = singularLabel;
}
