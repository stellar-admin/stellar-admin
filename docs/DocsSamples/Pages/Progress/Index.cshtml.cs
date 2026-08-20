using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocsSamples.Pages.Progress;

public class Index : PageModel
{
    public static List<ProgressFile> Files =>
        [
            new ProgressFile("1", "passport-scan.pdf", 45, "2m 30s"),
            new ProgressFile("2", "visa-application.pdf", 78, "45s"),
            new ProgressFile("3", "itinerary-draft.docx", 12, "5m 12s"),
            new ProgressFile("4", "hotel-voucher.jpg", 100, "complete"),
        ];

    public void OnGet() { }

    public record ProgressFile(string Id, string Name, int Progress, string TimeRemaining);
}
