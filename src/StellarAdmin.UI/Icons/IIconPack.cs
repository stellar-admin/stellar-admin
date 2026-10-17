namespace StellarAdmin.UI.Icons;

/// <summary>
///     Supplies a set of icons and the icon names used for semantic roles.
/// </summary>
/// <seealso cref="StellarAdminUIBuilder.AddIconPack{TIconPack}()" />
public interface IIconPack
{
    /// <summary>
    ///     Returns the icons that the icon pack supplies.
    /// </summary>
    /// <returns>A dictionary of icon definitions, keyed by icon name.</returns>
    IReadOnlyDictionary<string, IconDefinition> GetIcons();

    /// <summary>
    ///     Returns the icon names that the icon pack uses for semantic roles.
    /// </summary>
    /// <returns>A dictionary of icon names, keyed by semantic role.</returns>
    IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings();
}
