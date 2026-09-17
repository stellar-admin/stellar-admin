namespace StellarAdmin.Icons;

public interface IIconPack
{
    /// <summary>
    ///     Returns the list of icon definitions supplied by the icon pack.
    /// </summary>
    /// <returns></returns>
    IDictionary<string, IconDefinition> GetIcons();

    /// <summary>
    ///     Returns the icon names used for semantic roles.
    /// </summary>
    IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings();
}
