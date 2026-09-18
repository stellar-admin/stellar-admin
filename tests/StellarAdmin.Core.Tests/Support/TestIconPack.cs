using StellarAdmin.Icons;

namespace StellarAdmin.Core.Tests.Support;

internal sealed class TestIconPack : IIconPack
{
    public IDictionary<string, IconDefinition> GetIcons() =>
        new Dictionary<string, IconDefinition>
        {
            ["check"] = new(new Dictionary<string, string> { ["data-test"] = "replacement" }, []),
        };

    public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings() =>
        new Dictionary<SemanticIconRole, string>();
}
