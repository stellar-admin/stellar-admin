using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocsSamples.Pages.RichTextEditor;

public class Index : PageModel
{
    public IndexModel Model { get; set; } = new();

    public void OnGet() { }

    public class IndexModel
    {
        public GuideModel Guide { get; set; } = new();
    }

    public class GuideModel
    {
        [Display(
            Name = "Tour highlights",
            Description = "Shown on the tour page under the itinerary.",
            Prompt = "What makes this tour special?"
        )]
        public string? Highlights { get; set; } =
            "<p>Walk the <strong>old town of Kyoto</strong> at dawn, before the crowds arrive.</p><ul><li><p>Fushimi Inari at sunrise</p></li><li><p>Tea ceremony in Gion</p></li></ul>";
    }
}
