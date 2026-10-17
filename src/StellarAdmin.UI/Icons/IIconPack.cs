namespace StellarAdmin.UI.Icons;

public interface IIconPack
{
    /// <summary>
    ///     Returns the icons supplied by the icon pack, keyed by name.
    /// </summary>
    IReadOnlyDictionary<string, IconDefinition> GetIcons();

    /// <summary>
    ///     Returns the icon names used for semantic roles.
    /// </summary>
    IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings();
}
