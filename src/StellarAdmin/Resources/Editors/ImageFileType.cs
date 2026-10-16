using System.Text;

namespace StellarAdmin.Resources.Editors;

// Recognises the image formats browsers commonly display from a file's first bytes, so an upload's
// declared type can be checked against its content
internal static class ImageFileType
{
    private const int HeaderLength = 32;

    public static string[] KnownTypes { get; } =
    ["image/jpeg", "image/png", "image/gif", "image/webp", "image/avif"];

    // The media type of a known format, or null for any other content
    public static async Task<string?> DetectAsync(
        Stream stream,
        CancellationToken cancellationToken
    )
    {
        var header = new byte[HeaderLength];
        var length = await stream.ReadAtLeastAsync(
            header,
            HeaderLength,
            throwOnEndOfStream: false,
            cancellationToken
        );

        return Detect(header.AsSpan(0, length));
    }

    private static string? Detect(ReadOnlySpan<byte> header)
    {
        if (header.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xD8, 0xFF]))
        {
            return "image/jpeg";
        }

        if (header.StartsWith((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
        {
            return "image/png";
        }

        if (header.StartsWith("GIF87a"u8) || header.StartsWith("GIF89a"u8))
        {
            return "image/gif";
        }

        if (
            header.Length >= 12
            && header.StartsWith("RIFF"u8)
            && header[8..12].SequenceEqual("WEBP"u8)
        )
        {
            return "image/webp";
        }

        return IsAvif(header) ? "image/avif" : null;
    }

    // An ISO media file whose ftyp box names an AVIF brand, as its major brand or a compatible one
    private static bool IsAvif(ReadOnlySpan<byte> header)
    {
        if (header.Length < 16 || !header[4..8].SequenceEqual("ftyp"u8))
        {
            return false;
        }

        var boxLength = Math.Min(
            header.Length,
            (header[0] << 24) | (header[1] << 16) | (header[2] << 8) | header[3]
        );
        for (var offset = 8; offset + 4 <= boxLength; offset += 4)
        {
            // The minor version follows the major brand and is not a brand
            if (offset == 12)
            {
                continue;
            }

            var brand = Encoding.ASCII.GetString(header[offset..(offset + 4)]);
            if (brand is "avif" or "avis")
            {
                return true;
            }
        }

        return false;
    }
}
