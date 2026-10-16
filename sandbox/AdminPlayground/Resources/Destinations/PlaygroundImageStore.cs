using Microsoft.AspNetCore.StaticFiles;

namespace AdminPlayground.Resources.Destinations;

// Saves uploaded images to a folder outside the project and serves them back. Sample values are
// files in wwwroot/images; uploads are named upload-{guid} so the two never collide.
public sealed class PlaygroundImageStore
{
    public const string UploadPrefix = "upload-";

    private readonly string _folder = Path.Combine(
        Path.GetTempPath(),
        "stellaradmin-playground-uploads"
    );

    public string? GetPath(string name)
    {
        var fileName = Path.GetFileName(name);
        var path = Path.Combine(_folder, fileName);

        return fileName.StartsWith(UploadPrefix, StringComparison.Ordinal) && File.Exists(path)
            ? path
            : null;
    }

    public static string GetUrl(string value) =>
        value.StartsWith(UploadPrefix, StringComparison.Ordinal)
            ? $"/playground-uploads/{Uri.EscapeDataString(value)}"
            : $"/images/{Uri.EscapeDataString(value)}";

    public async Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_folder);
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var name = $"{UploadPrefix}{Guid.NewGuid():N}{extension}";
        await using var stream = File.Create(Path.Combine(_folder, name));
        await file.CopyToAsync(stream, cancellationToken);

        return name;
    }

    public static string GetContentType(string path) =>
        new FileExtensionContentTypeProvider().TryGetContentType(path, out var type)
            ? type
            : "application/octet-stream";
}
