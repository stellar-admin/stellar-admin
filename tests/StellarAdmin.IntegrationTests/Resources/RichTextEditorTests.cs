using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Editors;
using static StellarAdmin.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.IntegrationTests.Resources;

public class RichTextEditorTests
{
    private const string EditUrl = "/stellaradmin/products/edit/7";
    private const string UploadUrl = "/stellaradmin/products/uploadimage?form=edit&field=Story";

    private const string UnsafeHtml =
        "<p onclick=\"alert(1)\">Read <a href=\"javascript:alert(2)\">this</a> or <a href=\"https://voyager.example\">that</a></p>"
        + "<script>alert(3)</script><p style=\"color: red\">Styled</p><iframe src=\"https://evil.example\"></iframe>";

    private const string SanitizedHtml =
        "<p>Read <a>this</a> or <a href=\"https://voyager.example\">that</a></p><p>Styled</p>";

    // The first bytes of a PNG, which is all the type check reads
    private static readonly byte[] Png =
    [
        0x89,
        0x50,
        0x4E,
        0x47,
        0x0D,
        0x0A,
        0x1A,
        0x0A,
        0,
        0,
        0,
        0,
    ];

    [Test]
    public async Task Edit_WithRichTextEditor_RendersEditorWithSettingsAndUploadUrl()
    {
        // Arrange
        await using var sut = await CreateAsync(fields =>
            fields
                .Add(model => model.Story)
                .UseRichTextEditor(editor =>
                {
                    Configure(editor);
                    editor.Placeholder = "Tell the product's story";
                    editor.Rows = 4;
                    editor.MaxFileSize = 1024 * 1024;
                    editor.AcceptedTypes = ["image/png"];
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var editor = document.RequiredElement("sel-rich-text-editor");
        await Assert.That(editor.GetAttribute("data-upload-url")).IsEqualTo(UploadUrl);
        await Assert.That(editor.GetAttribute("data-max-file-size")).IsEqualTo("1048576");
        await Assert.That(editor.GetAttribute("data-accept")).IsEqualTo("image/png");
        var textarea = editor.RequiredElement("textarea");
        await Assert.That(textarea.GetAttribute("name")).IsEqualTo("Entity.Story");
        await Assert
            .That(textarea.GetAttribute("placeholder"))
            .IsEqualTo("Tell the product's story");
        await Assert.That(textarea.GetAttribute("rows")).IsEqualTo("4");
        await Assert
            .That(textarea.TextContent.Trim())
            .IsEqualTo("<p>Bound in <strong>Kyoto</strong>.</p>");
        await Assert
            .That(editor.RequiredElement("script").GetAttribute("src"))
            .StartsWith("/_content/StellarAdmin.UI/rich-text-editor.");
    }

    [Test]
    public async Task Edit_WithHtmlDataType_RendersRichTextEditorWithoutImages()
    {
        // Arrange
        await using var sut = await CreateAsync(fields => fields.Add(model => model.Notes));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var editor = document.RequiredElement("sel-rich-text-editor");
        await Assert
            .That(editor.RequiredElement("textarea").GetAttribute("name"))
            .IsEqualTo("Entity.Notes");
        await Assert.That(editor.HasAttribute("data-upload-url")).IsFalse();
        await Assert.That(editor.QuerySelector("[data-action=image]")).IsNull();
    }

    [Test]
    public async Task Save_WithUnsafeHtml_SavesSanitizedHtml()
    {
        // Arrange
        var stories = new ProductStories();
        await using var sut = await CreateAsync(
            fields =>
            {
                fields.Add(model => model.Story).UseRichTextEditor();
                fields.Add(model => model.Notes);
            },
            stories
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, EditUrl);
        values["Entity.Story"] = UnsafeHtml;
        values["Entity.Notes"] = UnsafeHtml;
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(EditUrl, content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(stories.Saved?.Story).IsEqualTo(SanitizedHtml);
        await Assert.That(stories.Saved?.Notes).IsEqualTo(SanitizedHtml);
    }

    [Test]
    public async Task Save_WithEditorMarkup_KeepsIt()
    {
        // Arrange
        const string html =
            "<h2>Kyoto</h2><p><strong>Bold</strong>, <em>italic</em>, <u>underlined</u>, <s>struck</s> and <code>code</code>.</p>"
            + "<ul><li><p>One</p></li></ul><ol><li><p>Two</p></li></ol><blockquote><p>Quote</p></blockquote><pre><code>let x;</code></pre><hr>"
            + "<table><tbody><tr><th colspan=\"2\" data-colwidth=\"120\"><p>Head</p></th></tr><tr><td><p>A</p></td><td><p>B</p></td></tr></tbody></table>"
            + "<img src=\"/uploads/kyoto.png\" width=\"320\" height=\"200\"><p><a href=\"/tours/kyoto\">Tour</a> <a href=\"mailto:hello@voyager.example\">Mail</a></p>";
        var stories = new ProductStories();
        await using var sut = await CreateAsync(
            fields => fields.Add(model => model.Story).UseRichTextEditor(),
            stories
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, EditUrl);
        values["Entity.Story"] = html;
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(EditUrl, content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(stories.Saved?.Story).IsEqualTo(html);
    }

    [Test]
    public async Task Save_WithSanitizerReplaced_UsesReplacement()
    {
        // Arrange
        var stories = new ProductStories();
        await using var sut = await CreateAsync(
            fields => fields.Add(model => model.Story).UseRichTextEditor(),
            stories,
            services => services.AddSingleton<IRichTextSanitizer, UpperCaseSanitizer>()
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, EditUrl);
        values["Entity.Story"] = "<p>quiet</p>";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(EditUrl, content);

        // Assert
        await Assert.That(stories.Saved?.Story).IsEqualTo("<P>QUIET</P>");
    }

    [Test]
    public async Task UploadImage_ToRichTextField_StoresItAndReturnsUrl()
    {
        // Arrange
        var stories = new ProductStories();
        await using var sut = await CreateAsync(
            fields => fields.Add(model => model.Story).UseRichTextEditor(Configure),
            stories
        );
        using var client = sut.GetTestClient();
        var token = (await PrepareForm(client, EditUrl))["__RequestVerificationToken"];

        // Act
        using var response = await UploadAsync(client, token, UploadUrl);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UploadResult>();
        await Assert.That(result?.Url).IsEqualTo("https://cdn.voyager.example/stored/lake.png");
        await Assert.That(stories.Stored).IsEquivalentTo(["lake.png"]);
    }

    [Test]
    public async Task UploadImage_ToRichTextFieldWithoutStoreImage_ReturnsNotFound()
    {
        // Arrange
        await using var sut = await CreateAsync(fields =>
            fields.Add(model => model.Story).UseRichTextEditor()
        );
        using var client = sut.GetTestClient();
        var token = (await PrepareForm(client, EditUrl))["__RequestVerificationToken"];

        // Act
        using var response = await UploadAsync(client, token, UploadUrl);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Startup_WithRichTextEditorWithoutAcceptedTypes_LogsProblem()
    {
        // Arrange

        // Act
        await using var app = await CreateAsync(fields =>
            fields
                .Add(model => model.Story)
                .UseRichTextEditor(editor =>
                {
                    Configure(editor);
                    editor.AcceptedTypes = [];
                })
        );

        // Assert
        var logs = app.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(
                logs.ConfigurationProblems.Any(log =>
                    log.Message.Contains(
                        "uses the default MaxFileSize or AcceptedTypes because its own aren't valid."
                    )
                )
            )
            .IsTrue();
    }

    private static void Configure(RichTextEditor editor) =>
        editor.StoreImage(
            async (file, services, _) =>
            {
                services.GetRequiredService<ProductStories>().Stored.Add(file.FileName);

                return await Task.FromResult($"https://cdn.voyager.example/stored/{file.FileName}");
            }
        );

    private static async Task<HttpResponseMessage> UploadAsync(
        HttpClient client,
        string token,
        string url
    )
    {
        using var file = new ByteArrayContent(Png);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        using var content = new MultipartFormDataContent { { file, "file", "lake.png" } };
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        request.Headers.Add("RequestVerificationToken", token);

        return await client.SendAsync(request);
    }

    private static Task<WebApplication> CreateAsync(
        Action<ResourceFieldsBuilder<ProductStoryModel>> fields,
        ProductStories? stories = null,
        Action<IServiceCollection>? configureServices = null
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit<ProductStoryModel, ProductStoryHandler>(edit =>
                    edit.Fields(fields)
                );
            },
            configureServices: services =>
            {
                services.AddSingleton(stories ?? new ProductStories());
                configureServices?.Invoke(services);
            }
        );

    private sealed class UpperCaseSanitizer : IRichTextSanitizer
    {
        public string Sanitize(string html) => html.ToUpperInvariant();
    }

    private sealed record UploadResult(string? Value, string? Url, string? Error);
}
