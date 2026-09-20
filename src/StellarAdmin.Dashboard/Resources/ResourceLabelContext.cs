namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     The resource labels available when generating page text.
/// </summary>
public sealed class ResourceLabelContext(string singularLabel, string pluralLabel)
{
    /// <summary>
    ///     The plural resource label.
    /// </summary>
    public string PluralLabel { get; } = pluralLabel;

    /// <summary>
    ///     The singular resource label.
    /// </summary>
    public string SingularLabel { get; } = singularLabel;
}
