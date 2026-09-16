namespace StellarAdmin.Icons;

public interface IIconPack
{
    IDictionary<string, IconDefinition> GetIcons();

    /// <summary>
    ///     Returns the icon names used for semantic roles.
    /// </summary>
    IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings()
    {
        return System.Collections.Immutable.ImmutableDictionary<SemanticIconRole, string>.Empty;
    }
}
