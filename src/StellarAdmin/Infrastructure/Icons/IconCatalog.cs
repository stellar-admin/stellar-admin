using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.Infrastructure.Icons;

// The registered icons in the payload an icon picker's icons-url answers with. The icons can't change once the
// options are configured, so the payload, its version and its compressed copies are built once, the copies on
// first request so a panel that never opens a picker never compresses
internal sealed class IconCatalog
{
    private readonly Lazy<byte[]> _brotli;
    private readonly Lazy<byte[]> _gzip;
    private readonly Lazy<(byte[] Json, string Version)> _payload;

    public IconCatalog(IOptions<IconOptions> iconOptions)
    {
        _payload = new Lazy<(byte[], string)>(() => Build(iconOptions.Value));
        _brotli = new Lazy<byte[]>(() =>
            Compress(Json, stream => new BrotliStream(stream, CompressionLevel.SmallestSize))
        );
        _gzip = new Lazy<byte[]>(() =>
            Compress(Json, stream => new GZipStream(stream, CompressionLevel.SmallestSize))
        );
    }

    public byte[] Brotli => _brotli.Value;

    public byte[] Gzip => _gzip.Value;

    public byte[] Json => _payload.Value.Json;

    public string Version => _payload.Value.Version;

    private static (byte[], string) Build(IconOptions icons)
    {
        using var buffer = new MemoryStream();
        // The response is only ever read as JSON, so the markup's angle brackets and quotes needn't be escaped
        using (
            var writer = new Utf8JsonWriter(
                buffer,
                new JsonWriterOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }
            )
        )
        {
            writer.WriteStartObject();
            writer.WriteStartArray("icons");
            foreach (var name in icons.GetIconNames().Order(StringComparer.Ordinal))
            {
                icons.TryGetIcon(name, out var icon);
                writer.WriteStartObject();
                writer.WriteString("name", name);
                writer.WriteString("svg", ToSvg(icon!));
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        var json = buffer.ToArray();
        var version = Convert.ToHexStringLower(SHA256.HashData(json))[..16];

        return (json, version);
    }

    private static byte[] Compress(byte[] data, Func<Stream, Stream> compress)
    {
        using var buffer = new MemoryStream();
        using (var stream = compress(buffer))
        {
            stream.Write(data);
        }

        return buffer.ToArray();
    }

    private static string ToSvg(IconDefinition icon)
    {
        // The picker parses the markup as XML, which needs the namespace an icon added in code may leave out
        var svg = new StringBuilder("<svg");
        if (!icon.Attributes.ContainsKey("xmlns"))
        {
            svg.Append(" xmlns=\"http://www.w3.org/2000/svg\"");
        }

        AppendAttributes(svg, icon.Attributes);
        svg.Append('>');
        foreach (var shape in icon.Shapes)
        {
            svg.Append('<').Append(shape.Name);
            AppendAttributes(svg, shape.Attributes);
            svg.Append("/>");
        }

        return svg.Append("</svg>").ToString();
    }

    private static void AppendAttributes(
        StringBuilder svg,
        IReadOnlyDictionary<string, string> attributes
    )
    {
        foreach (var (name, value) in attributes)
        {
            svg.Append(' ')
                .Append(name)
                .Append("=\"")
                .Append(WebUtility.HtmlEncode(value))
                .Append('"');
        }
    }
}
