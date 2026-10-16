using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Tests.Resources.Editors;

public class ImageFileTypeTests
{
    [Test]
    [Arguments(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0x10 }, "image/jpeg")]
    [Arguments(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0 }, "image/png")]
    [Arguments(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 1, 0 }, "image/gif")]
    [Arguments(
        new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0, 0, 0, 0x57, 0x45, 0x42, 0x50, 0x56 },
        "image/webp"
    )]
    // ftyp avif, as the major brand
    [Arguments(
        new byte[]
        {
            0,
            0,
            0,
            0x1C,
            0x66,
            0x74,
            0x79,
            0x70,
            0x61,
            0x76,
            0x69,
            0x66,
            0,
            0,
            0,
            0,
            0x61,
            0x76,
            0x69,
            0x66,
            0x6D,
            0x69,
            0x66,
            0x31,
        },
        "image/avif"
    )]
    // ftyp mif1 with avif among the compatible brands
    [Arguments(
        new byte[]
        {
            0,
            0,
            0,
            0x18,
            0x66,
            0x74,
            0x79,
            0x70,
            0x6D,
            0x69,
            0x66,
            0x31,
            0,
            0,
            0,
            0,
            0x6D,
            0x69,
            0x66,
            0x31,
            0x61,
            0x76,
            0x69,
            0x66,
        },
        "image/avif"
    )]
    public async Task DetectAsync_WithKnownFormat_ReturnsItsType(byte[] content, string expected)
    {
        // Arrange
        using var stream = new MemoryStream(content);

        // Act
        var type = await ImageFileType.DetectAsync(stream, CancellationToken.None);

        // Assert
        await Assert.That(type).IsEqualTo(expected);
    }

    [Test]
    // Text, an empty file, an HEIC file (ftyp heic), and a minor version that spells avif
    [Arguments(new byte[] { 0x50, 0x61, 0x63, 0x6B, 0x20, 0x6C, 0x69, 0x67, 0x68, 0x74 })]
    [Arguments(new byte[] { })]
    [Arguments(
        new byte[]
        {
            0,
            0,
            0,
            0x18,
            0x66,
            0x74,
            0x79,
            0x70,
            0x68,
            0x65,
            0x69,
            0x63,
            0,
            0,
            0,
            0,
            0x6D,
            0x69,
            0x66,
            0x31,
            0x68,
            0x65,
            0x69,
            0x63,
        }
    )]
    [Arguments(
        new byte[]
        {
            0,
            0,
            0,
            0x18,
            0x66,
            0x74,
            0x79,
            0x70,
            0x68,
            0x65,
            0x69,
            0x63,
            0x61,
            0x76,
            0x69,
            0x66,
            0x6D,
            0x69,
            0x66,
            0x31,
            0x68,
            0x65,
            0x69,
            0x63,
        }
    )]
    public async Task DetectAsync_WithOtherContent_ReturnsNull(byte[] content)
    {
        // Arrange
        using var stream = new MemoryStream(content);

        // Act
        var type = await ImageFileType.DetectAsync(stream, CancellationToken.None);

        // Assert
        await Assert.That(type).IsNull();
    }
}
