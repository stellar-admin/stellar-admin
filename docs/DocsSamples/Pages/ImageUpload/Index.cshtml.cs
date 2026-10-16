using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocsSamples.Pages.ImageUpload;

// The demos' upload endpoint: it keeps uploads in a temporary folder and serves them back, after a
// short pause so the progress shows. The demos have no form to carry an antiforgery token.
[IgnoreAntiforgeryToken]
public class Index : PageModel
{
    public const string UploadUrl = "/ImageUpload?handler=Upload";

    private static readonly string Folder = Path.Combine(Path.GetTempPath(), "docs-samples-uploads");

    public void OnGet() { }

    public async Task<IActionResult> OnPostUpload(IFormFile? file)
    {
        if (file is null || !file.ContentType.StartsWith("image/", StringComparison.Ordinal))
        {
            return BadRequest(new { error = "Choose an image to upload." });
        }

        Directory.CreateDirectory(Folder);
        var name = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
        await using (var stream = System.IO.File.Create(Path.Combine(Folder, name)))
        {
            await file.CopyToAsync(stream);
        }

        await Task.Delay(1200);

        return new JsonResult(new { value = name, url = $"/ImageUpload?handler=File&name={name}" });
    }

    public IActionResult OnGetFile(string name)
    {
        var path = Path.Combine(Folder, Path.GetFileName(name));

        return System.IO.File.Exists(path) ? PhysicalFile(path, "image/jpeg") : NotFound();
    }
}
