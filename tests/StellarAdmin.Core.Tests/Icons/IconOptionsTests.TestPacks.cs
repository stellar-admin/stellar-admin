using StellarAdmin.Icons;

namespace StellarAdmin.Core.Tests.Icons;

public partial class IconOptionsTests
{
    private sealed class InvalidPack : IIconPack
    {
        public IDictionary<string, IconDefinition> GetIcons() =>
            new Dictionary<string, IconDefinition> { ["new-icon"] = ReplacementPack.Icon };

        public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings() =>
            new Dictionary<SemanticIconRole, string> { [SemanticIconRole.Close] = "absent" };
    }

    private sealed class LegacyPack : IIconPack
    {
        public IDictionary<string, IconDefinition> GetIcons() =>
            new Dictionary<string, IconDefinition>();

        public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings()
        {
            return new Dictionary<SemanticIconRole, string>();
        }
    }

    private sealed class MappingOnlyPack : IIconPack
    {
        public IDictionary<string, IconDefinition> GetIcons() =>
            new Dictionary<string, IconDefinition>();

        public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings() =>
            new Dictionary<SemanticIconRole, string> { [SemanticIconRole.Close] = "test-dots" };
    }

    private sealed class OverridePack : IIconPack
    {
        public IDictionary<string, IconDefinition> GetIcons() =>
            new Dictionary<string, IconDefinition>
            {
                ["test-dots"] = new(
                    new Dictionary<string, string> { ["data-test-icon"] = "override" },
                    []
                ),
            };

        public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings()
        {
            return new Dictionary<SemanticIconRole, string>();
        }
    }

    private sealed class PartiallyValidPack : IIconPack
    {
        public IDictionary<string, IconDefinition> GetIcons() =>
            new Dictionary<string, IconDefinition>
            {
                ["x"] = ReplacementPack.Icon,
                ["new-icon"] = ReplacementPack.Icon,
            };

        public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings() =>
            new Dictionary<SemanticIconRole, string>
            {
                [SemanticIconRole.Close] = "new-icon",
                [SemanticIconRole.PaginationEllipsis] = "absent",
            };
    }

    private sealed class ReplacementPack : IIconPack
    {
        public static IconDefinition Icon { get; } =
            new(new Dictionary<string, string> { ["data-test-icon"] = "replacement" }, []);

        public IDictionary<string, IconDefinition> GetIcons() =>
            new Dictionary<string, IconDefinition> { ["test-dots"] = Icon };

        public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings() =>
            new Dictionary<SemanticIconRole, string>
            {
                [SemanticIconRole.PaginationEllipsis] = "TEST-DOTS",
            };
    }

    private sealed class UnreadableMappingsPack : IIconPack
    {
        public IDictionary<string, IconDefinition> GetIcons() =>
            new Dictionary<string, IconDefinition> { ["test-dots"] = ReplacementPack.Icon };

        public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings() =>
            throw new InvalidOperationException("Mappings must not be read.");
    }
}
