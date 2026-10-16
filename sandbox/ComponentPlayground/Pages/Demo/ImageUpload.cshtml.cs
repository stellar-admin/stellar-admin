using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComponentPlayground.Pages.Demo;

// The upload endpoint stores files in a temporary folder and serves them back, after a pause so
// the progress shows.
public class ImageUpload : PageModel
{
    private static readonly string Folder = Path.Combine(Path.GetTempPath(), "playground-uploads");

    [BindProperty]
    public string? Photo { get; set; } = "lisbon.jpg";

    [BindProperty]
    public string? Cover { get; set; }

    [BindProperty]
    public string? Thumbnail { get; set; } = "chiang-mai-portrait.jpg";

    [BindProperty]
    public string? Attachment { get; set; } = "cape-town-panorama.jpg";

    public string? Posted { get; private set; }

    public static string? Url(string? value) =>
        string.IsNullOrEmpty(value) ? null
        : value.StartsWith("upload/") ? $"/Demo/ImageUpload?handler=File&name={value[7..]}"
        : $"/images/{value}";

    public void OnGet() { }

    public void OnPost()
    {
        Posted = $"Photo={Photo}; Cover={Cover}; Thumbnail={Thumbnail}; Attachment={Attachment}";
    }

    public async Task<IActionResult> OnPostUpload(IFormFile file, int delay = 1500)
    {
        if (!file.ContentType.StartsWith("image/"))
        {
            return BadRequest(new { error = $"{file.FileName} isn't an image." });
        }

        Directory.CreateDirectory(Folder);
        var name = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        await using (var stream = System.IO.File.Create(Path.Combine(Folder, name)))
        {
            await file.CopyToAsync(stream);
        }

        await Task.Delay(delay);
        var value = $"upload/{name}";

        return new JsonResult(new { value, url = Url(value) });
    }

    public IActionResult OnPostUploadFail(IFormFile file) =>
        StatusCode(507, new { error = "The image store is full." });

    public IActionResult OnGetFile(string name) =>
        PhysicalFile(Path.Combine(Folder, Path.GetFileName(name)), "image/jpeg");
}
