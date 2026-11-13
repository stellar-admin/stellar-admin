using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using StellarAdmin.Generators;
using TUnit.Assertions;
using TUnit.Core;

namespace StellarAdmin.Generators.Tests;

public class DataAttributeTextGeneratorTests
{
    private static readonly MetadataReference[] References =
    [
        .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path)),
    ];

    [Test]
    public async Task Initialize_WhenMemberNameHasSeveralWords_UsesKebabCase()
    {
        // Arrange
        const string source = """
            namespace Sample;

            [StellarAdmin.Generators.GenerateDataAttributeText]
            public enum ButtonVariant
            {
                Default,
                OutlineDestructive,
                IconExtraSmall,
            }
            """;

        // Act
        var generated = Generate(source);

        // Assert
        var text = generated["Sample.ButtonVariant.DataAttributeText.g.cs"];
        await Assert.That(text).Contains("global::Sample.ButtonVariant.Default => \"default\",");
        await Assert
            .That(text)
            .Contains(
                "global::Sample.ButtonVariant.OutlineDestructive => \"outline-destructive\","
            );
        await Assert
            .That(text)
            .Contains("global::Sample.ButtonVariant.IconExtraSmall => \"icon-extra-small\",");
    }

    [Test]
    public async Task Initialize_WhenMemberHasDataAttributeText_UsesItsText()
    {
        // Arrange
        const string source = """
            using StellarAdmin.Generators;

            namespace Sample;

            [GenerateDataAttributeText]
            public enum AvatarSize
            {
                Default,

                [DataAttributeText("sm")]
                Small,
            }
            """;

        // Act
        var generated = Generate(source);

        // Assert
        var text = generated["Sample.AvatarSize.DataAttributeText.g.cs"];
        await Assert.That(text).Contains("internal static class AvatarSizeDataAttributeText");
        await Assert.That(text).Contains("extension(global::Sample.AvatarSize value)");
        await Assert.That(text).Contains("global::Sample.AvatarSize.Default => \"default\",");
        await Assert.That(text).Contains("global::Sample.AvatarSize.Small => \"sm\",");
    }

    [Test]
    public async Task Initialize_WhenEnumIsNotMarked_GeneratesOnlyTheAttributes()
    {
        // Arrange
        const string source = """
            namespace Sample;

            public enum AvatarSize
            {
                Default,
                Small,
            }
            """;

        // Act
        var generated = Generate(source);

        // Assert
        await Assert.That(generated.Keys).IsEquivalentTo(["DataAttributeTextAttributes.g.cs"]);
    }

    private static Dictionary<string, string> Generate(string source)
    {
        var compilation = CSharpCompilation.Create(
            "Sample",
            [CSharpSyntaxTree.ParseText(source)],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var result = CSharpGeneratorDriver
            .Create(new DataAttributeTextGenerator())
            .RunGenerators(compilation)
            .GetRunResult()
            .Results.Single();

        return result.GeneratedSources.ToDictionary(
            generatedSource => generatedSource.HintName,
            generatedSource => generatedSource.SourceText.ToString()
        );
    }
}
