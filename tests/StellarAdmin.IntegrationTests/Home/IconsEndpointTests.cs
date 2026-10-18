using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.IntegrationTests.Home;

public class IconsEndpointTests
{
    private const string IconsUrl = "/stellaradmin/home/icons";

    [Test]
    public async Task Icons_ReturnsEveryRegisteredIconWithItsMarkup()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();
        var registered = sut.Services.GetRequiredService<IOptions<IconOptions>>().Value;

        // Act
        using var response = await client.GetAsync(IconsUrl);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(response.Content.Headers.ContentType?.MediaType)
            .IsEqualTo("application/json");
        await Assert.That(response.Content.Headers.ContentEncoding).IsEmpty();
        var icons = ReadIcons(await response.Content.ReadAsByteArrayAsync());
        await Assert.That(icons.Keys).IsEquivalentTo(registered.GetIconNames());
        await Assert
            .That(icons["anchor"])
            .StartsWith("<svg xmlns=\"http://www.w3.org/2000/svg\"")
            .And.EndsWith("</svg>");
    }

    [Test]
    [Arguments("br")]
    [Arguments("gzip")]
    public async Task Icons_AcceptingCompression_ReturnsCompressedPayload(string encoding)
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();
        var plain = await client.GetByteArrayAsync(IconsUrl);
        using var request = new HttpRequestMessage(HttpMethod.Get, IconsUrl);
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(encoding));

        // Act
        using var response = await client.SendAsync(request);

        // Assert
        await Assert.That(response.Content.Headers.ContentEncoding).IsEquivalentTo([encoding]);
        await Assert.That(response.Headers.Vary).Contains("Accept-Encoding");
        var body = await response.Content.ReadAsByteArrayAsync();
        await Assert.That(body.Length).IsLessThan(plain.Length / 5);
        await Assert.That(Decompress(body, encoding)).IsEquivalentTo(plain);
    }

    [Test]
    public async Task Icons_WithCurrentVersion_IsCachedForGood()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();
        var version = (await client.GetAsync(IconsUrl)).Headers.ETag!.Tag.Trim('"');

        // Act
        using var response = await client.GetAsync($"{IconsUrl}?v={version}");

        // Assert
        await Assert.That(response.Headers.CacheControl?.Private).IsTrue();
        await Assert.That(response.Headers.CacheControl?.MaxAge).IsEqualTo(TimeSpan.FromDays(365));
        await Assert
            .That(response.Headers.CacheControl?.Extensions.Select(e => e.Name))
            .Contains("immutable");
    }

    [Test]
    [Arguments("")]
    [Arguments("?v=0123456789abcdef")]
    public async Task Icons_WithoutCurrentVersion_IsRevalidated(string query)
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(IconsUrl + query);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(response.Headers.CacheControl?.NoCache).IsTrue();
    }

    [Test]
    public async Task Icons_WithMatchingETag_ReturnsNotModified()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();
        var etag = (await client.GetAsync(IconsUrl)).Headers.ETag!;
        using var request = new HttpRequestMessage(HttpMethod.Get, IconsUrl);
        request.Headers.IfNoneMatch.Add(etag);

        // Act
        using var response = await client.SendAsync(request);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotModified);
    }

    [Test]
    public async Task Icons_AddedInCode_AreIncludedWithNamespace()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureServices: services =>
                services.Configure<IconOptions>(options =>
                {
                    options.ClearIcons();
                    options.AddIcon(
                        "voyager-compass",
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
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var icons = ReadIcons(await client.GetByteArrayAsync(IconsUrl));

        // Assert
        await Assert
            .That(icons)
            .IsEquivalentTo(
                new Dictionary<string, string>
                {
                    ["voyager-compass"] =
                        "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\"><circle cx=\"12\" cy=\"12\" r=\"10\"/></svg>",
                }
            );
    }

    private static byte[] Decompress(byte[] body, string encoding)
    {
        using var input = new MemoryStream(body);
        using Stream stream =
            encoding == "br"
                ? new BrotliStream(input, CompressionMode.Decompress)
                : new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        stream.CopyTo(output);

        return output.ToArray();
    }

    private static Dictionary<string, string> ReadIcons(byte[] json)
    {
        using var document = JsonDocument.Parse(json);

        return document
            .RootElement.GetProperty("icons")
            .EnumerateArray()
            .ToDictionary(
                icon => icon.GetProperty("name").GetString()!,
                icon => icon.GetProperty("svg").GetString()!
            );
    }
}
