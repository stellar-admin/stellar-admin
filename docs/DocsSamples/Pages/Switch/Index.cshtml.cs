using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocsSamples.Pages.Switch;

public class Index : PageModel
{
    public IndexModel Model { get; set; } = new IndexModel();

    public void OnGet() { }

    public class IndexModel
    {
        public ModelBindingModel ModelBinding { get; set; } = new ModelBindingModel();
    }

    public class ModelBindingModel
    {
        [Display(
            Name = "Flight alerts",
            Description = "Receive an email when a flight on your itinerary changes."
        )]
        public bool EmailNotifications { get; set; } = true;
    }
}
