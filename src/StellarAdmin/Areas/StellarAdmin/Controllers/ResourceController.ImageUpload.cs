using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Areas.StellarAdmin.Controllers;

internal sealed partial class ResourceController<TResource>
{
    // An image upload's body is the file and the multipart form around it
    private const long ImageUploadOverhead = 64 * 1024;

    // The upload's response is read by the image upload's script, whatever the host's JSON settings
    private static readonly JsonSerializerOptions ImageUploadJsonOptions = new(
        JsonSerializerDefaults.Web
    );

    // No parameters, so model binding leaves the body unread until its limit is the field's
    [HttpPost]
    public async Task<IActionResult> UploadImage()
    {
        var cancellationToken = HttpContext.RequestAborted;
        string? fieldName = Request.Query["field"];
        if (FindImageField(Request.Query["form"], fieldName) is not { } editor)
        {
            return NotFound();
        }

        if (!Request.HasFormContentType)
        {
            return BadRequest();
        }

        // Room for the file and the multipart form around it
        var bodyLimit = editor.MaxFileSize + ImageUploadOverhead;
        if (
            HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>() is
            { IsReadOnly: false } bodySize
        )
        {
            bodySize.MaxRequestBodySize = bodyLimit;
        }

        IFormCollection form;
        try
        {
            form = await Request.ReadFormAsync(
                new FormOptions { MultipartBodyLengthLimit = bodyLimit },
                cancellationToken
            );
        }
        catch (Exception exception)
            when (exception is InvalidDataException or BadHttpRequestException)
        {
            return ImageUploadError(
                StatusCodes.Status413PayloadTooLarge,
                $"The image is larger than the {FormatFileSize(editor.MaxFileSize)} limit."
            );
        }

        try
        {
            await HttpContext
                .RequestServices.GetRequiredService<IAntiforgery>()
                .ValidateRequestAsync(HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            return BadRequest();
        }

        if (form.Files.GetFile("file") is not { Length: > 0 } file)
        {
            return ImageUploadError(StatusCodes.Status400BadRequest, "Choose an image to upload.");
        }

        var fileName = Path.GetFileName(file.FileName);
        if (file.Length > editor.MaxFileSize)
        {
            return ImageUploadError(
                StatusCodes.Status413PayloadTooLarge,
                $"{fileName} is {FormatFileSize(file.Length)}. The limit is {FormatFileSize(editor.MaxFileSize)}."
            );
        }

        // A known format is recognised by its content; any other content goes by its declared type
        string? detectedType;
        await using (var stream = file.OpenReadStream())
        {
            detectedType = await ImageFileType.DetectAsync(stream, cancellationToken);
        }

        var type =
            detectedType
            ?? (
                ImageFileType.KnownTypes.Contains(
                    file.ContentType,
                    StringComparer.OrdinalIgnoreCase
                )
                    ? null
                    : file.ContentType
            );
        if (type is null || !editor.AcceptedTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
        {
            return ImageUploadError(
                StatusCodes.Status415UnsupportedMediaType,
                $"{fileName} can't be used. Choose a different type of image."
            );
        }

        var (value, url) = await editor.StoreAsync(
            file,
            HttpContext.RequestServices,
            cancellationToken
        );
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"StoreImage for the field {fieldName} returned no value."
            );
        }

        return Json(new { value, url }, ImageUploadJsonOptions);
    }

    // A size as the image upload's script writes it, such as 5 MB or 640 KB
    private static string FormatFileSize(long bytes) =>
        bytes >= 1024 * 1024
            ? $"{Math.Round(bytes / (1024d * 1024), 1).ToString(CultureInfo.InvariantCulture)} MB"
            : $"{Math.Max(1, (long)Math.Round(bytes / 1024d)).ToString(CultureInfo.InvariantCulture)} KB";

    // The editable image field of the create or edit form, which must store uploads
    private IImageUploadEditor? FindImageField(string? form, string? fieldName)
    {
        var fields = form?.ToLowerInvariant() switch
        {
            "create" => _resourceOptions.Create?.Fields,
            "edit" when _resourceOptions.KeySelector is not null => _resourceOptions.Edit?.Fields,
            _ => null,
        };

        return
            fields?.FirstOrDefault(options => options.FieldName == fieldName)
                is { IsNeverEditable: false, Editor: IImageUploadEditor { HasStore: true } editor }
            ? editor
            : null;
    }

    private JsonResult ImageUploadError(int statusCode, string error)
    {
        var result = Json(new { error }, ImageUploadJsonOptions);
        result.StatusCode = statusCode;

        return result;
    }
}
