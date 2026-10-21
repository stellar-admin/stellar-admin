using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;
using static StellarAdmin.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.IntegrationTests.Resources;

public class ImageEditorTests
{
    private const string EditUrl = "/stellaradmin/products/edit/7";
    private const string UploadUrl = "/stellaradmin/products/uploadimage?form=edit&field=Photo";

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
    public async Task Edit_WithImageEditor_RendersUploadWithResolvedUrlAndSettings()
    {
        // Arrange
        await using var sut = await CreateAsync(fields =>
        {
            fields.Add(model => model.Photo).UseEditor<ImageEditor>(Configure);
            fields
                .Add(model => model.Thumbnail)
                .UseEditor<ImageEditor>(editor =>
                {
                    Configure(editor);
                    editor.Layout = ImageEditorLayout.Compact;
                    editor.MaxFileSize = 1024 * 1024;
                    editor.AcceptedTypes = ["image/png", "image/webp"];
                });
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var photo = Upload(document, "Photo");
        await Assert.That(photo.GetAttribute("data-layout")).IsEqualTo("dropzone");
        await Assert.That(photo.GetAttribute("data-upload-url")).IsEqualTo(UploadUrl);
        await Assert.That(photo.GetAttribute("data-max-file-size")).IsEqualTo("5242880");
        await Assert
            .That(photo.GetAttribute("data-accept"))
            .IsEqualTo("image/jpeg,image/png,image/webp,image/gif,image/avif");
        await Assert
            .That(photo.RequiredElement(".sa-image img").GetAttribute("src"))
            .IsEqualTo("https://cdn.voyager.example/notebook.jpg");
        var thumbnail = Upload(document, "Thumbnail");
        await Assert.That(thumbnail.GetAttribute("data-layout")).IsEqualTo("compact");
        await Assert.That(thumbnail.GetAttribute("data-max-file-size")).IsEqualTo("1048576");
        await Assert.That(thumbnail.GetAttribute("data-accept")).IsEqualTo("image/png,image/webp");
    }

    [Test]
    public async Task Edit_WithImageEditorClassNames_AddsClassesToParts()
    {
        // Arrange
        await using var sut = await CreateAsync(fields =>
            fields
                .Add(model => model.Photo)
                .UseEditor<ImageEditor>(editor =>
                {
                    Configure(editor);
                    editor.ClassNames.Control = "max-w-md";
                    editor.ClassNames.Image = "shadow-lg";
                    editor.ClassNames.Note = "italic";
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var photo = Upload(document, "Photo");
        await Assert.That(photo.ClassList.Contains("max-w-md")).IsTrue();
        await Assert
            .That(photo.RequiredElement(".sa-image").ClassList.Contains("shadow-lg"))
            .IsTrue();
        await Assert
            .That(
                photo
                    .RequiredElement("[data-slot='image-upload-note']")
                    .ClassList.Contains("italic")
            )
            .IsTrue();
    }

    [Test]
    public async Task UploadImage_WithAcceptedImage_StoresItAndReturnsValueAndUrl()
    {
        // Arrange
        var photos = new ProductPhotos();
        await using var sut = await CreateAsync(
            fields => fields.Add(model => model.Photo).UseEditor<ImageEditor>(Configure),
            photos
        );
        using var client = sut.GetTestClient();
        var token = await PrepareTokenAsync(client);

        // Act
        using var response = await UploadAsync(client, token, Png, "lake.png", "image/png");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UploadResult>();
        await Assert.That(result?.Value).IsEqualTo("stored/lake.png");
        await Assert.That(result?.Url).IsEqualTo("https://cdn.voyager.example/stored/lake.png");
        await Assert
            .That(photos.Stored)
            .IsEquivalentTo([new StoredPhoto("lake.png", "image/png", Png.Length)]);
    }

    [Test]
    public async Task UploadImage_WithoutAntiforgeryToken_ReturnsBadRequest()
    {
        // Arrange
        var photos = new ProductPhotos();
        await using var sut = await CreateAsync(
            fields => fields.Add(model => model.Photo).UseEditor<ImageEditor>(Configure),
            photos
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await UploadAsync(client, null, Png, "lake.png", "image/png");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(photos.Stored).IsEmpty();
    }

    [Test]
    public async Task UploadImage_LargerThanMaxFileSize_ReturnsErrorWithoutStoring()
    {
        // Arrange
        var photos = new ProductPhotos();
        await using var sut = await CreateAsync(
            fields =>
                fields
                    .Add(model => model.Photo)
                    .UseEditor<ImageEditor>(editor =>
                    {
                        Configure(editor);
                        editor.MaxFileSize = 8;
                    }),
            photos
        );
        using var client = sut.GetTestClient();
        var token = await PrepareTokenAsync(client);

        // Act
        using var response = await UploadAsync(client, token, Png, "lake.png", "image/png");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.RequestEntityTooLarge);
        var result = await response.Content.ReadFromJsonAsync<UploadResult>();
        await Assert.That(result?.Error).IsEqualTo("lake.png is 1 KB. The limit is 1 KB.");
        await Assert.That(photos.Stored).IsEmpty();
    }

    [Test]
    [Arguments("image/png", "notes.png")]
    [Arguments("text/plain", "notes.txt")]
    public async Task UploadImage_WhoseContentIsNotAnAcceptedImage_ReturnsErrorWithoutStoring(
        string contentType,
        string fileName
    )
    {
        // Arrange
        var photos = new ProductPhotos();
        await using var sut = await CreateAsync(
            fields => fields.Add(model => model.Photo).UseEditor<ImageEditor>(Configure),
            photos
        );
        using var client = sut.GetTestClient();
        var token = await PrepareTokenAsync(client);

        // Act
        using var response = await UploadAsync(
            client,
            token,
            "Pack light."u8.ToArray(),
            fileName,
            contentType
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.UnsupportedMediaType);
        var result = await response.Content.ReadFromJsonAsync<UploadResult>();
        await Assert
            .That(result?.Error)
            .IsEqualTo($"{fileName} can't be used. Choose a different type of image.");
        await Assert.That(photos.Stored).IsEmpty();
    }

    [Test]
    public async Task UploadImage_OfTypeNotAccepted_ReturnsErrorWithoutStoring()
    {
        // Arrange
        var photos = new ProductPhotos();
        await using var sut = await CreateAsync(
            fields =>
                fields
                    .Add(model => model.Photo)
                    .UseEditor<ImageEditor>(editor =>
                    {
                        Configure(editor);
                        editor.AcceptedTypes = ["image/jpeg"];
                    }),
            photos
        );
        using var client = sut.GetTestClient();
        var token = await PrepareTokenAsync(client);

        // Act
        using var response = await UploadAsync(client, token, Png, "lake.png", "image/png");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.UnsupportedMediaType);
        await Assert.That(photos.Stored).IsEmpty();
    }

    [Test]
    [Arguments("/stellaradmin/products/uploadimage?form=edit&field=Name")]
    [Arguments("/stellaradmin/products/uploadimage?form=create&field=Photo")]
    [Arguments("/stellaradmin/products/uploadimage?form=edit&field=Missing")]
    public async Task UploadImage_ToFieldWithoutUploads_ReturnsNotFound(string url)
    {
        // Arrange
        await using var sut = await CreateAsync(fields =>
        {
            fields.Add(model => model.Name);
            fields.Add(model => model.Photo).UseEditor<ImageEditor>(Configure);
        });
        using var client = sut.GetTestClient();
        var token = await PrepareTokenAsync(client);

        // Act
        using var response = await UploadAsync(client, token, Png, "lake.png", "image/png", url);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UploadImage_ToResourceRequiringAuthorization_RejectsAnonymousRequest()
    {
        // Arrange
        var photos = new ProductPhotos();
        await using var sut = await CreateAsync(
            fields => fields.Add(model => model.Photo).UseEditor<ImageEditor>(Configure),
            photos,
            resource => resource.RequireAuthorization(),
            TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await UploadAsync(client, null, Png, "lake.png", "image/png");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(photos.Stored).IsEmpty();
    }

    [Test]
    public async Task Save_WithUploadedValue_SavesValue()
    {
        // Arrange
        var photos = new ProductPhotos();
        await using var sut = await CreateAsync(
            fields => fields.Add(model => model.Photo).UseEditor<ImageEditor>(Configure),
            photos
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, EditUrl);
        values["Entity.Photo"] = "stored/lake.png";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(EditUrl, content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(photos.Saved?.Photo).IsEqualTo("stored/lake.png");
    }

    [Test]
    public async Task EditRow_WithImageEditor_RendersThumbnailButton()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "notebook.jpg", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                {
                    edit.Fields(fields =>
                        fields.Add(product => product.Name).UseEditor<ImageEditor>(Configure)
                    );
                    edit.UseInlineRow();
                });
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/editrow/7");

        // Assert
        var upload = document.RequiredElement("sel-image-upload");
        await Assert.That(upload.GetAttribute("data-layout")).IsEqualTo("cell");
        await Assert
            .That(upload.GetAttribute("data-upload-url"))
            .IsEqualTo("/stellaradmin/products/uploadimage?form=edit&field=Name");
        await Assert
            .That(upload.RequiredElement("input[type='hidden']").GetAttribute("name"))
            .IsEqualTo("rows[7].Name");
    }

    [Test]
    public async Task Startup_WithImageEditorWithoutStoreImage_LogsProblem()
    {
        // Arrange

        // Act
        await using var app = await CreateAsync(fields =>
            fields.Add(model => model.Photo).UseEditor<ImageEditor>()
        );

        // Assert
        var logs = app.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(
                logs.ConfigurationProblems.Any(log =>
                    log.Message.Contains(
                        "The Photo field isn't on the edit form because its image editor has nowhere to store uploads."
                    )
                )
            )
            .IsTrue();
    }

    private static void Configure(ImageEditor editor)
    {
        editor.ResolveUrl(value => $"https://cdn.voyager.example/{value}");
        editor.StoreImage(
            async (file, services, _) =>
            {
                services
                    .GetRequiredService<ProductPhotos>()
                    .Stored.Add(new(file.FileName, file.ContentType, file.Length));

                return await Task.FromResult($"stored/{file.FileName}");
            }
        );
    }

    private static IElement Upload(IDocument document, string name) =>
        document
            .RequiredElement($"input[type='hidden'][name='Entity.{name}']")
            .Closest("sel-image-upload")!;

    private static async Task<string> PrepareTokenAsync(HttpClient client) =>
        (await PrepareForm(client, EditUrl))["__RequestVerificationToken"];

    // The image upload's request: the file, with the form's token in a header
    private static async Task<HttpResponseMessage> UploadAsync(
        HttpClient client,
        string? token,
        byte[] bytes,
        string fileName,
        string contentType,
        string url = UploadUrl
    )
    {
        using var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        using var content = new MultipartFormDataContent { { file, "file", fileName } };
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        if (token is not null)
        {
            request.Headers.Add("RequestVerificationToken", token);
        }

        return await client.SendAsync(request);
    }

    private static Task<WebApplication> CreateAsync(
        Action<ResourceFieldsBuilder<ProductPhotoModel>> fields,
        ProductPhotos? photos = null,
        Action<ResourceBuilder<Product>>? configure = null,
        Action<IServiceCollection>? configureServices = null
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit<ProductPhotoModel, ProductPhotoHandler>(edit =>
                    edit.Fields(fields)
                );
                configure?.Invoke(resource);
            },
            configureServices: services =>
            {
                services.AddSingleton(photos ?? new ProductPhotos());
                configureServices?.Invoke(services);
            }
        );

    private sealed record UploadResult(string? Value, string? Url, string? Error);
}
