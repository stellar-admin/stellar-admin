using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.Options;
using StellarAdmin.Infrastructure.Icons;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.Tests.Infrastructure.Icons;

public class IconCatalogTests
{
    [Test]
    public async Task Json_ListsIconsByNameWithTheirMarkup()
    {
        // Arrange
        var sut = new IconCatalog(Options.Create(CreateIcons()));

        // Act
        var json = Encoding.UTF8.GetString(sut.Json);

        // Assert
        await Assert
            .That(json)
            .IsEqualTo(
                """{"icons":[{"name":"compass","svg":"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\"><circle cx=\"12\" cy=\"12\" r=\"10\"/></svg>"},{"name":"mountain","svg":"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\" data-label=\"a &amp; b\"><path d=\"m8 3 4 8 5-5 5 15H2L8 3z\"/></svg>"}]}"""
            );
    }

    [Test]
    public async Task Version_IsTheSameForTheSameIcons()
    {
        // Arrange
        var first = new IconCatalog(Options.Create(CreateIcons()));
        var second = new IconCatalog(Options.Create(CreateIcons()));

        // Act
        var version = first.Version;

        // Assert
        await Assert.That(version).Matches("^[0-9a-f]{16}$");
        await Assert.That(version).IsEqualTo(second.Version);
    }

    [Test]
    public async Task Version_ChangesWithTheIcons()
    {
        // Arrange
        var icons = CreateIcons();
        icons.RemoveIcon("mountain");
        var sut = new IconCatalog(Options.Create(icons));

        // Act
        var version = sut.Version;

        // Assert
        await Assert
            .That(version)
            .IsNotEqualTo(new IconCatalog(Options.Create(CreateIcons())).Version);
    }

    [Test]
    public async Task CompressedCopies_DecompressToJson()
    {
        // Arrange
        var sut = new IconCatalog(Options.Create(new IconOptions()));

        // Act
        var brotli = Decompress(
            new BrotliStream(new MemoryStream(sut.Brotli), CompressionMode.Decompress)
        );
        var gzip = Decompress(
            new GZipStream(new MemoryStream(sut.Gzip), CompressionMode.Decompress)
        );

        // Assert
        await Assert.That(brotli).IsEquivalentTo(sut.Json);
        await Assert.That(gzip).IsEquivalentTo(sut.Json);
    }

    private static IconOptions CreateIcons()
    {
        var icons = new IconOptions();
        icons.ClearIcons();
        icons.AddIcon(
            "mountain",
            new IconDefinition(
                new Dictionary<string, string>
                {
                    ["xmlns"] = "http://www.w3.org/2000/svg",
                    ["viewBox"] = "0 0 24 24",
                    ["data-label"] = "a & b",
                },
                [
                    new SvgShape(
                        "path",
                        new Dictionary<string, string> { ["d"] = "m8 3 4 8 5-5 5 15H2L8 3z" }
                    ),
                ]
            )
        );
        icons.AddIcon(
            "compass",
            new IconDefinition(
                new Dictionary<string, string> { ["viewBox"] = "0 0 24 24" },
                [
                    new SvgShape(
                        "circle",
                        new Dictionary<string, string>
                        {
                            ["cx"] = "12",
                            ["cy"] = "12",
                            ["r"] = "10",
                        }
                    ),
                ]
            )
        );

        return icons;
    }

    private static byte[] Decompress(Stream stream)
    {
        using (stream)
        {
            using var output = new MemoryStream();
            stream.CopyTo(output);

            return output.ToArray();
        }
    }
}
