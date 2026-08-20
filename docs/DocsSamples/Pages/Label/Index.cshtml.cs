using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocsSamples.Pages.Label;

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
        [Display(Name = "Traveller name")]
        public string? TravellerName { get; set; } = "Ibn Battuta";
    }
}
